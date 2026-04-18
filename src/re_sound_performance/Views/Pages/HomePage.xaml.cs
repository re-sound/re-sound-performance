using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Presets;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages;

public partial class HomePage : System.Windows.Controls.Page
{
    private readonly TweakEngine _engine;
    private readonly TweakStateCache _cache;
    private readonly DetectionContext _detection;
    private readonly PresetRunner _presetRunner;
    private EventHandler<TweakStateChangedEventArgs>? _stateHandler;
    private EventHandler<TweakProbingProgressEventArgs>? _progressHandler;
    private EventHandler? _detectionHandler;
    private CancellationTokenSource? _presetRunCts;

    public HomePage()
    {
        InitializeComponent();
        var services = ((App)Application.Current).Services;
        _engine = services.GetRequiredService<TweakEngine>();
        _cache = _engine.StateCache;
        _detection = services.GetRequiredService<DetectionContext>();
        _presetRunner = services.GetRequiredService<PresetRunner>();

        InitializePresetCards();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void InitializePresetCards()
    {
        var safe = PresetCatalog.Get(PresetKind.Safe);
        var balanced = PresetCatalog.Get(PresetKind.Balanced);
        var competitive = PresetCatalog.Get(PresetKind.Competitive);

        PresetSafeName.Text = safe.Name;
        PresetSafeTagline.Text = safe.Tagline;
        PresetSafeCount.Text = $"{safe.TweakIds.Count} tweaks";

        PresetBalancedName.Text = balanced.Name;
        PresetBalancedTagline.Text = balanced.Tagline;
        PresetBalancedCount.Text = $"{balanced.TweakIds.Count} tweaks";

        PresetCompetitiveName.Text = competitive.Name;
        PresetCompetitiveTagline.Text = competitive.Tagline;
        PresetCompetitiveCount.Text = $"{competitive.TweakIds.Count} tweaks";
    }

    private async void PresetCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (_presetRunCts is not null)
        {
            return;
        }

        if (sender is not FrameworkElement element || element.Tag is not string tag)
        {
            return;
        }

        if (!Enum.TryParse<PresetKind>(tag, ignoreCase: true, out var kind))
        {
            return;
        }

        var preset = PresetCatalog.Get(kind);
        var preview = BuildPresetPreview(preset);
        var confirm = await preview.ShowAsync();
        if (confirm != ContentDialogResult.Primary)
        {
            return;
        }

        await RunPresetAsync(preset);
    }

    private ContentDialog BuildPresetPreview(Preset preset)
    {
        var list = new ItemsControl
        {
            Margin = new Thickness(0, 8, 0, 0)
        };

        foreach (var tweakId in preset.TweakIds)
        {
            var tweak = _engine.Resolve(tweakId);
            var status = _cache.GetOrUnknown(tweakId);
            var name = tweak?.Metadata.Name ?? tweakId;
            var gate = tweak is null ? new TweakGateDecision(true, null) : TweakGate.Evaluate(tweak.Metadata, _detection.AntiCheat);

            var tagText = tweak is null
                ? "Unavailable in this build"
                : !gate.Allowed
                    ? "Blocked by anti-cheat"
                    : status switch
                    {
                        TweakStatus.Applied => "Already applied",
                        TweakStatus.PartiallyApplied => "Partially applied",
                        TweakStatus.Unavailable => "Not applicable",
                        TweakStatus.NotApplied => "Will apply",
                        _ => "Unknown state"
                    };

            var tagColorKey = tweak is null
                ? "ReSound.Surface.Muted"
                : !gate.Allowed
                    ? "ReSound.Status.Danger"
                    : status switch
                    {
                        TweakStatus.Applied => "ReSound.Status.Success",
                        TweakStatus.PartiallyApplied => "ReSound.Status.Warning",
                        TweakStatus.Unavailable => "ReSound.Surface.Muted",
                        _ => "ReSound.Accent.Primary"
                    };

            var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 12,
                Foreground = ResourceBrush("ReSound.Text.Primary"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlock, 0);
            row.Children.Add(nameBlock);

            var tag = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8, 2, 8, 2),
                Background = ResourceBrush(tagColorKey),
                Child = new TextBlock
                {
                    Text = tagText,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White
                }
            };
            Grid.SetColumn(tag, 1);
            row.Children.Add(tag);

            list.Items.Add(row);
        }

        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 380,
            Content = list
        };

        var header = new StackPanel();
        header.Children.Add(new TextBlock
        {
            Text = preset.Description,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12),
            Foreground = ResourceBrush("ReSound.Text.Secondary")
        });
        header.Children.Add(scroll);

        return new ContentDialog
        {
            Title = $"Preset: {preset.Name}",
            Content = header,
            PrimaryButtonText = "Apply preset",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
    }

    private async Task RunPresetAsync(Preset preset)
    {
        _presetRunCts = new CancellationTokenSource();
        PresetRunRow.Visibility = Visibility.Visible;
        PresetRunText.Text = $"Applying preset: {preset.Name}";
        PresetRunBar.Width = 4;
        PresetRunSteps.Items.Clear();
        PresetCancelButton.IsEnabled = true;

        SetPresetCardsEnabled(false);

        var progress = new Progress<PresetProgress>(step => Dispatcher.Invoke(() => RenderStep(step)));

        try
        {
            var summary = await Task.Run(() => _presetRunner.RunAsync(preset, progress, _presetRunCts.Token)).ConfigureAwait(true);
            PresetRunText.Text = summary.Cancelled
                ? $"Preset {preset.Name} cancelled: {summary.Applied} applied, {summary.Blocked} blocked, {summary.Failed} failed"
                : $"Preset {preset.Name} done: {summary.Applied} applied, {summary.Skipped} skipped, {summary.Blocked} blocked, {summary.Failed} failed, {summary.Unavailable} unavailable";
        }
        catch (Exception ex)
        {
            PresetRunText.Text = $"Preset {preset.Name} crashed: {ex.Message}";
        }
        finally
        {
            _presetRunCts.Dispose();
            _presetRunCts = null;
            PresetCancelButton.IsEnabled = false;
            SetPresetCardsEnabled(true);
        }
    }

    private void PresetCancelButton_Click(object sender, RoutedEventArgs e)
    {
        _presetRunCts?.Cancel();
        PresetCancelButton.IsEnabled = false;
    }

    private void RenderStep(PresetProgress step)
    {
        var width = step.Total > 0 ? Math.Max(4, 900.0 * step.Completed / step.Total) : 4;
        PresetRunBar.Width = Math.Min(width, double.PositiveInfinity);

        var (label, colorKey) = step.Outcome switch
        {
            PresetStepOutcome.Applied => ("Applied", "ReSound.Status.Success"),
            PresetStepOutcome.Skipped => ("Skipped", "ReSound.Surface.Muted"),
            PresetStepOutcome.Blocked => ("Blocked", "ReSound.Status.Danger"),
            PresetStepOutcome.Failed => ("Failed", "ReSound.Status.Danger"),
            PresetStepOutcome.Unavailable => ("N/A", "ReSound.Surface.Muted"),
            _ => ("Unknown", "ReSound.Surface.Muted")
        };

        var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var counter = new TextBlock
        {
            Text = $"{step.Completed}/{step.Total}",
            Foreground = ResourceBrush("ReSound.Text.Muted"),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
            MinWidth = 36
        };
        Grid.SetColumn(counter, 0);
        row.Children.Add(counter);

        var name = new TextBlock
        {
            Text = step.CurrentTweakName,
            Foreground = ResourceBrush("ReSound.Text.Primary"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(name, 1);
        row.Children.Add(name);

        var tag = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 2, 8, 2),
            Background = ResourceBrush(colorKey),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = label,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            },
            ToolTip = step.Message
        };
        Grid.SetColumn(tag, 2);
        row.Children.Add(tag);

        PresetRunSteps.Items.Add(row);
    }

    private void SetPresetCardsEnabled(bool enabled)
    {
        PresetCardSafe.IsEnabled = enabled;
        PresetCardBalanced.IsEnabled = enabled;
        PresetCardCompetitive.IsEnabled = enabled;
        PresetCardSafe.Opacity = enabled ? 1.0 : 0.55;
        PresetCardBalanced.Opacity = enabled ? 1.0 : 0.55;
        PresetCardCompetitive.Opacity = enabled ? 1.0 : 0.55;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Render();
        RenderDetection();

        _stateHandler = (_, _) => Dispatcher.BeginInvoke(new Action(Render));
        _progressHandler = (_, args) => Dispatcher.BeginInvoke(new Action(() => UpdateProgress(args)));
        _detectionHandler = (_, _) => Dispatcher.BeginInvoke(new Action(RenderDetection));

        _cache.StateChanged += _stateHandler;
        _cache.ProbingProgress += _progressHandler;
        _detection.Changed += _detectionHandler;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_stateHandler is not null)
        {
            _cache.StateChanged -= _stateHandler;
            _stateHandler = null;
        }

        if (_progressHandler is not null)
        {
            _cache.ProbingProgress -= _progressHandler;
            _progressHandler = null;
        }

        if (_detectionHandler is not null)
        {
            _detection.Changed -= _detectionHandler;
            _detectionHandler = null;
        }
    }

    private void RenderDetection()
    {
        var hw = _detection.Hardware;
        if (hw is null)
        {
            HwPrimary.Text = "Detecting...";
            HwSecondary.Text = string.Empty;
        }
        else
        {
            HwPrimary.Text = $"{hw.CpuModel}";
            var storageLabel = hw.PrimaryStorage switch
            {
                StorageKind.Nvme => "NVMe",
                StorageKind.Ssd => "SSD",
                StorageKind.Hdd => "HDD",
                _ => "Unknown storage"
            };
            HwSecondary.Text = $"{hw.GpuModel}  -  {hw.RamGb} GB RAM  -  {storageLabel}  -  {hw.OsBuild}";
        }

        var ac = _detection.AntiCheat;
        AcDetails.Items.Clear();
        if (ac is null)
        {
            AcPrimary.Text = "Scanning...";
            return;
        }

        if (!ac.HasAny)
        {
            AcPrimary.Text = "None detected";
            AcPrimary.Foreground = ResourceBrush("ReSound.Status.Success");
            return;
        }

        AcPrimary.Text = string.Join(" + ", EnumerateFlags(ac.Installed));
        AcPrimary.Foreground = ResourceBrush("ReSound.Status.Warning");

        foreach (var detail in ac.Details)
        {
            AcDetails.Items.Add(new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 3, 10, 3),
                Margin = new Thickness(0, 6, 6, 0),
                Background = ResourceBrush("ReSound.Surface.Hover"),
                BorderBrush = ResourceBrush("ReSound.Surface.Divider"),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = detail,
                    FontSize = 11,
                    Foreground = ResourceBrush("ReSound.Text.Secondary")
                }
            });
        }
    }

    private static IEnumerable<string> EnumerateFlags(AntiCheat flags)
    {
        if ((flags & AntiCheat.Vanguard) == AntiCheat.Vanguard) yield return "Vanguard";
        if ((flags & AntiCheat.FaceitAc) == AntiCheat.FaceitAc) yield return "FACEIT AC";
        if ((flags & AntiCheat.EasyAntiCheat) == AntiCheat.EasyAntiCheat) yield return "EAC";
        if ((flags & AntiCheat.BattlEye) == AntiCheat.BattlEye) yield return "BattlEye";
    }

    private static Brush ResourceBrush(string key)
    {
        if (Application.Current?.TryFindResource(key) is Brush brush)
        {
            return brush;
        }
        return Brushes.Gray;
    }

    private void UpdateProgress(TweakProbingProgressEventArgs args)
    {
        if (args.IsComplete || args.Total == 0)
        {
            ProbingRow.Visibility = Visibility.Collapsed;
            return;
        }

        ProbingRow.Visibility = Visibility.Visible;
        ProbingText.Text = $"Scanning system state… {args.Completed}/{args.Total}";
        var ratio = args.Total > 0 ? (double)args.Completed / args.Total : 0;
        ProbingBar.Width = Math.Max(8, 360 * ratio);
    }

    private void Render()
    {
        var tweaks = _engine.AvailableTweaks.ToList();
        var probed = tweaks
            .Select(t => (Tweak: t, Status: _cache.GetOrUnknown(t.Metadata.Id)))
            .Where(s => s.Status != TweakStatus.Unknown)
            .ToList();

        var applied = probed.Count(s => s.Status == TweakStatus.Applied);
        var partial = probed.Count(s => s.Status is TweakStatus.PartiallyApplied or TweakStatus.Unavailable);
        var highRiskApplied = probed.Count(s => s.Status == TweakStatus.Applied && s.Tweak.Metadata.Risk != TweakRisk.Safe);

        TotalTweaksText.Text = tweaks.Count.ToString();
        AppliedTweaksText.Text = applied.ToString();
        PartialTweaksText.Text = partial.ToString();
        HighRiskAppliedText.Text = highRiskApplied.ToString();

        RiskBreakdownList.Items.Clear();
        foreach (var risk in new[] { TweakRisk.Safe, TweakRisk.Medium, TweakRisk.High })
        {
            var forRisk = tweaks.Where(t => t.Metadata.Risk == risk).ToList();
            if (forRisk.Count == 0)
            {
                continue;
            }

            var appliedForRisk = forRisk.Count(t => _cache.GetOrUnknown(t.Metadata.Id) == TweakStatus.Applied);
            RiskBreakdownList.Items.Add(BuildProgressRow(
                RiskLabel(risk),
                RiskBrush(risk),
                appliedForRisk,
                forRisk.Count));
        }

        CategoryBreakdownList.Items.Clear();
        foreach (var group in tweaks.GroupBy(t => t.Metadata.Category).OrderBy(g => CategoryOrder(g.Key)))
        {
            var appliedForCategory = group.Count(t => _cache.GetOrUnknown(t.Metadata.Id) == TweakStatus.Applied);
            CategoryBreakdownList.Items.Add(BuildProgressRow(
                CategoryLabel(group.Key),
                CategoryBrush(group.Key),
                appliedForCategory,
                group.Count()));
        }
    }

    private static UIElement BuildProgressRow(string label, Brush accent, int applied, int total)
    {
        var panel = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

        var labelBlock = new TextBlock
        {
            Text = label,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelBlock, 0);
        panel.Children.Add(labelBlock);

        var progress = new Grid { VerticalAlignment = VerticalAlignment.Center, Height = 8 };
        progress.Children.Add(new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            CornerRadius = new CornerRadius(4)
        });
        progress.Children.Add(new Border
        {
            Background = accent,
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = total > 0 ? Math.Max(4, 360.0 * applied / total) : 0
        });
        Grid.SetColumn(progress, 1);
        panel.Children.Add(progress);

        var countBlock = new TextBlock
        {
            Text = $"{applied}/{total}",
            FontSize = 12,
            Opacity = 0.75,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(countBlock, 2);
        panel.Children.Add(countBlock);

        return panel;
    }

    private static string RiskLabel(TweakRisk risk) => risk switch
    {
        TweakRisk.Safe => "Safe",
        TweakRisk.Medium => "Medium",
        TweakRisk.High => "High",
        _ => "Unknown"
    };

    private static Brush RiskBrush(TweakRisk risk)
    {
        var key = risk switch
        {
            TweakRisk.Safe => "ReSound.Risk.Safe",
            TweakRisk.Medium => "ReSound.Risk.Medium",
            TweakRisk.High => "ReSound.Risk.High",
            _ => "ReSound.Surface.Muted"
        };

        return (Application.Current?.TryFindResource(key) as Brush) ?? Brushes.Gray;
    }

    private static string CategoryLabel(TweakCategory category) => category switch
    {
        TweakCategory.System => "System",
        TweakCategory.Privacy => "Privacy",
        TweakCategory.Debloat => "Debloat",
        TweakCategory.Gpu => "GPU",
        TweakCategory.Network => "Network",
        TweakCategory.Input => "Input",
        TweakCategory.Power => "Power",
        TweakCategory.Game => "Games",
        _ => category.ToString()
    };

    private static Brush CategoryBrush(TweakCategory category)
    {
        var key = category switch
        {
            TweakCategory.System => "ReSound.Category.System",
            TweakCategory.Privacy => "ReSound.Category.Privacy",
            TweakCategory.Debloat => "ReSound.Category.Debloat",
            TweakCategory.Gpu => "ReSound.Category.Gpu",
            TweakCategory.Network => "ReSound.Category.Network",
            TweakCategory.Input => "ReSound.Category.Input",
            TweakCategory.Power => "ReSound.Category.Power",
            TweakCategory.Game => "ReSound.Category.Game",
            _ => "ReSound.Surface.Muted"
        };

        return (Application.Current?.TryFindResource(key) as Brush) ?? Brushes.Gray;
    }

    private static int CategoryOrder(TweakCategory category) => category switch
    {
        TweakCategory.System => 0,
        TweakCategory.Power => 1,
        TweakCategory.Network => 2,
        TweakCategory.Input => 3,
        TweakCategory.Gpu => 4,
        TweakCategory.Privacy => 5,
        TweakCategory.Debloat => 6,
        TweakCategory.Game => 7,
        _ => 99
    };
}
