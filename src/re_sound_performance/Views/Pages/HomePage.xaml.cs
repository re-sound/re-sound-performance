using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages;

public partial class HomePage : Page
{
    private readonly TweakEngine _engine;

    public HomePage()
    {
        InitializeComponent();
        _engine = ((App)Application.Current).Services.GetRequiredService<TweakEngine>();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var tweaks = _engine.AvailableTweaks.ToList();

        var statuses = await Task.Run(async () =>
        {
            var collected = new List<(ITweak Tweak, TweakStatus Status)>(tweaks.Count);
            foreach (var tweak in tweaks)
            {
                var status = await _engine.ProbeAsync(tweak.Metadata.Id).ConfigureAwait(false);
                collected.Add((tweak, status));
            }

            return collected;
        }).ConfigureAwait(true);

        var applied = statuses.Count(s => s.Status == TweakStatus.Applied);
        var partial = statuses.Count(s => s.Status is TweakStatus.PartiallyApplied or TweakStatus.Unavailable);
        var highRiskApplied = statuses.Count(s => s.Status == TweakStatus.Applied && s.Tweak.Metadata.Risk != TweakRisk.Safe);

        TotalTweaksText.Text = tweaks.Count.ToString();
        AppliedTweaksText.Text = applied.ToString();
        PartialTweaksText.Text = partial.ToString();
        HighRiskAppliedText.Text = highRiskApplied.ToString();

        RiskBreakdownList.Items.Clear();
        foreach (var risk in new[] { TweakRisk.Safe, TweakRisk.Medium, TweakRisk.High })
        {
            var forRisk = statuses.Where(s => s.Tweak.Metadata.Risk == risk).ToList();
            if (forRisk.Count == 0)
            {
                continue;
            }

            var appliedForRisk = forRisk.Count(s => s.Status == TweakStatus.Applied);
            RiskBreakdownList.Items.Add(BuildProgressRow(
                RiskLabel(risk),
                RiskBrush(risk),
                appliedForRisk,
                forRisk.Count));
        }

        CategoryBreakdownList.Items.Clear();
        foreach (var group in statuses.GroupBy(s => s.Tweak.Metadata.Category).OrderBy(g => CategoryOrder(g.Key)))
        {
            var appliedForCategory = group.Count(s => s.Status == TweakStatus.Applied);
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

    private static SolidColorBrush RiskBrush(TweakRisk risk) => risk switch
    {
        TweakRisk.Safe => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
        TweakRisk.Medium => new SolidColorBrush(Color.FromRgb(0xBF, 0x76, 0x20)),
        TweakRisk.High => new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C)),
        _ => new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61))
    };

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

    private static SolidColorBrush CategoryBrush(TweakCategory category) => category switch
    {
        TweakCategory.System => new SolidColorBrush(Color.FromRgb(0x0A, 0xA2, 0xA2)),
        TweakCategory.Privacy => new SolidColorBrush(Color.FromRgb(0x8E, 0x44, 0xE0)),
        TweakCategory.Debloat => new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)),
        TweakCategory.Gpu => new SolidColorBrush(Color.FromRgb(0x7C, 0xB3, 0x42)),
        TweakCategory.Network => new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4)),
        TweakCategory.Input => new SolidColorBrush(Color.FromRgb(0xEC, 0x40, 0x7A)),
        TweakCategory.Power => new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x00)),
        TweakCategory.Game => new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
        _ => new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61))
    };

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
