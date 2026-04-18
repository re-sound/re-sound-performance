using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using re_sound_performance.Controls;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages.Shared;

internal static class TweakPageLoader
{
    public static async Task PopulateAsync(TweakEngine engine, ItemsControl host, IReadOnlyCollection<TweakCategory> allowedCategories)
    {
        host.Items.Clear();

        var grouped = engine.AvailableTweaks
            .Where(t => allowedCategories.Contains(t.Metadata.Category))
            .GroupBy(t => t.Metadata.Category)
            .OrderBy(g => CategoryOrder(g.Key))
            .ToList();

        if (grouped.Count == 0)
        {
            host.Items.Add(new TextBlock
            {
                Text = "No tweaks available for this page yet.",
                Opacity = 0.6,
                Margin = new Thickness(0, 8, 0, 0)
            });
            return;
        }

        foreach (var group in grouped)
        {
            host.Items.Add(BuildCategoryHeader(group.Key, group.Count()));

            foreach (var tweak in group.OrderBy(t => t.Metadata.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var card = new TweakCard { Metadata = tweak.Metadata };
                var capturedTweak = tweak;
                card.ToggleRequested += async (_, _) => await OnToggleAsync(engine, card, capturedTweak).ConfigureAwait(true);
                host.Items.Add(card);

                var status = await Task.Run(() => engine.ProbeAsync(tweak.Metadata.Id)).ConfigureAwait(true);
                card.SetAppliedSilently(status == TweakStatus.Applied);
                card.Status = MapInitialStatus(status);
            }
        }
    }

    private static async Task OnToggleAsync(TweakEngine engine, TweakCard card, ITweak tweak)
    {
        var willApply = !card.IsApplied;

        card.SetToggleEnabled(false);
        card.Status = willApply ? TweakCardStatus.Applying : TweakCardStatus.Reverting;
        card.StatusMessage = null;

        try
        {
            var result = willApply
                ? await Task.Run(() => engine.ApplyAsync(tweak.Metadata.Id)).ConfigureAwait(true)
                : await Task.Run(() => engine.RevertAsync(tweak.Metadata.Id)).ConfigureAwait(true);

            var verified = await Task.Run(() => engine.ProbeAsync(tweak.Metadata.Id)).ConfigureAwait(true);
            card.SetAppliedSilently(verified == TweakStatus.Applied);

            if (!result.Success)
            {
                card.Status = TweakCardStatus.Failed;
                card.StatusMessage = result.Message ?? "Operation failed.";
                return;
            }

            card.Status = (willApply, verified) switch
            {
                (true, TweakStatus.Applied) => TweakCardStatus.Success,
                (true, TweakStatus.PartiallyApplied) => TweakCardStatus.Partial,
                (true, TweakStatus.Unavailable) => TweakCardStatus.Unavailable,
                (true, TweakStatus.NotApplied) => TweakCardStatus.Failed,
                (false, TweakStatus.NotApplied) => TweakCardStatus.Success,
                (false, TweakStatus.Applied) => TweakCardStatus.Failed,
                (false, TweakStatus.Unavailable) => TweakCardStatus.Unavailable,
                (false, TweakStatus.PartiallyApplied) => TweakCardStatus.Partial,
                _ => TweakCardStatus.Unavailable
            };

            card.StatusMessage = card.Status == TweakCardStatus.Failed && string.IsNullOrWhiteSpace(result.Message)
                ? "Post-apply verification mismatch: changes did not take effect."
                : result.Message;
        }
        catch (Exception ex)
        {
            card.Status = TweakCardStatus.Failed;
            card.StatusMessage = ex.Message;
        }
        finally
        {
            card.SetToggleEnabled(true);
        }
    }

    private static TweakCardStatus MapInitialStatus(TweakStatus status) => status switch
    {
        TweakStatus.PartiallyApplied => TweakCardStatus.Partial,
        TweakStatus.Unavailable => TweakCardStatus.Unavailable,
        _ => TweakCardStatus.Idle
    };

    private static UIElement BuildCategoryHeader(TweakCategory category, int count)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(2, 16, 0, 8)
        };

        panel.Children.Add(new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = CategoryAccentBrush(category),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });

        panel.Children.Add(new TextBlock
        {
            Text = CategoryLabel(category),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"  ({count})",
            FontSize = 14,
            Opacity = 0.55,
            VerticalAlignment = VerticalAlignment.Center
        });

        return panel;
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

    private static SolidColorBrush CategoryAccentBrush(TweakCategory category) => category switch
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
}
