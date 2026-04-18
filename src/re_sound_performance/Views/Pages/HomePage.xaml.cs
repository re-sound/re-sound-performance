using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages;

public partial class HomePage : Page
{
    private readonly TweakEngine _engine;
    private readonly TweakStateCache _cache;
    private EventHandler<TweakStateChangedEventArgs>? _stateHandler;
    private EventHandler<TweakProbingProgressEventArgs>? _progressHandler;

    public HomePage()
    {
        InitializeComponent();
        var services = ((App)Application.Current).Services;
        _engine = services.GetRequiredService<TweakEngine>();
        _cache = _engine.StateCache;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Render();

        _stateHandler = (_, _) => Dispatcher.BeginInvoke(new Action(Render));
        _progressHandler = (_, args) => Dispatcher.BeginInvoke(new Action(() => UpdateProgress(args)));

        _cache.StateChanged += _stateHandler;
        _cache.ProbingProgress += _progressHandler;
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
