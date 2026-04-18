using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Games;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages.Shared;

internal sealed class GamePageModel
{
    public required GameId Game { get; init; }
    public required GameInstallation Installation { get; init; }
    public required RecommendedConfig Recommendation { get; init; }
    public required bool VanguardInstalled { get; init; }
    public required IReadOnlyList<TweakMetadata> VanguardBlockedTweaks { get; init; }
}

internal static class GamePageLoader
{
    public static async Task<GamePageModel> LoadAsync(GameId game)
    {
        var services = ((App)Application.Current).Services;
        var detector = services.GetRequiredService<IGameDetector>();
        var detectionContext = services.GetRequiredService<DetectionContext>();
        var engine = services.GetRequiredService<TweakEngine>();

        var installation = await detector.DetectAsync(game).ConfigureAwait(true);
        var vanguardInstalled = detectionContext.AntiCheat?.Has(AntiCheat.Vanguard) ?? false;
        var blocked = VanguardBlockedTweaks.ListFor(engine.AvailableTweaks);

        return new GamePageModel
        {
            Game = game,
            Installation = installation,
            Recommendation = GameRecommendations.For(game),
            VanguardInstalled = vanguardInstalled,
            VanguardBlockedTweaks = blocked
        };
    }

    public static async Task<GameConfigWriteResult> WriteRecommendedAsync(GameInstallation install, RecommendedConfig recommendation)
    {
        var services = ((App)Application.Current).Services;
        var writer = services.GetRequiredService<IGameConfigWriter>();
        return await writer.WriteRecommendedAsync(install, recommendation).ConfigureAwait(true);
    }

    public static UIElement BuildDetectionBadge(GameInstallation install)
    {
        var statusText = install.Installed ? "Installed" : "Not detected";
        var launcherText = install.Launcher switch
        {
            GameLauncher.Steam => "Steam",
            GameLauncher.RiotClient => "Riot Client",
            GameLauncher.EaApp => "EA App",
            GameLauncher.Origin => "Origin",
            _ => "Unknown launcher"
        };

        var badge = new StackPanel { Orientation = Orientation.Horizontal };
        badge.Children.Add(StatusPill(statusText, install.Installed ? "ReSound.Status.Success" : "ReSound.Surface.Muted"));
        if (install.Installed)
        {
            badge.Children.Add(StatusPill(launcherText, "ReSound.Accent.Deep"));
        }
        return badge;
    }

    public static UIElement BuildInstallPathRow(GameInstallation install)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        panel.Children.Add(new TextBlock
        {
            Text = install.InstallPath ?? "Install path: not located",
            FontSize = 12,
            Foreground = (Brush)Application.Current.TryFindResource("ReSound.Text.Secondary"),
            TextWrapping = TextWrapping.Wrap
        });

        if (!string.IsNullOrWhiteSpace(install.Notes))
        {
            panel.Children.Add(new TextBlock
            {
                Text = install.Notes,
                FontSize = 11,
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Status.Warning"),
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
        }

        return panel;
    }

    public static UIElement BuildSettingsList(IReadOnlyList<RecommendedSetting> settings)
    {
        var list = new ItemsControl();
        foreach (var setting in settings)
        {
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var nameBlock = new TextBlock
            {
                Text = setting.Key,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Text.Primary"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            var valueBlock = new TextBlock
            {
                Text = setting.Value,
                FontSize = 12,
                FontFamily = new FontFamily("Consolas, Cascadia Mono, monospace"),
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Accent.Light"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);

            var explain = new TextBlock
            {
                Text = setting.Explanation,
                FontSize = 11,
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Text.Muted"),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(explain, 2);
            grid.Children.Add(explain);

            list.Items.Add(grid);
        }

        return list;
    }

    public static UIElement BuildNotesList(IReadOnlyList<string> notes)
    {
        var list = new ItemsControl();
        foreach (var note in notes)
        {
            list.Items.Add(new TextBlock
            {
                Text = "- " + note,
                FontSize = 12,
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Text.Secondary"),
                Margin = new Thickness(0, 2, 0, 2),
                TextWrapping = TextWrapping.Wrap
            });
        }
        return list;
    }

    public static UIElement BuildBlockedTweaksList(IReadOnlyList<TweakMetadata> blocked)
    {
        var list = new ItemsControl();
        foreach (var metadata in blocked)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = metadata.Name,
                FontSize = 12,
                Foreground = (Brush)Application.Current.TryFindResource("ReSound.Text.Primary"),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(nameBlock, 0);
            row.Children.Add(nameBlock);

            row.Children.Add(WithColumn(StatusPill(metadata.Category.ToString(), "ReSound.Category." + metadata.Category), 1));
            list.Items.Add(row);
        }
        return list;
    }

    public static string BuildLaunchOptionsLine(string options) => options;

    private static Border StatusPill(string text, string brushKey) => new()
    {
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(10, 2, 10, 2),
        Margin = new Thickness(0, 0, 6, 0),
        Background = (Brush)(Application.Current.TryFindResource(brushKey) ?? Brushes.Gray),
        Child = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White
        }
    };

    private static T WithColumn<T>(T element, int column) where T : UIElement
    {
        Grid.SetColumn(element, column);
        return element;
    }
}
