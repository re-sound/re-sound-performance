using System.Windows;
using System.Windows.Controls;
using re_sound_performance.Core.Games;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class ValorantPage : System.Windows.Controls.Page
{
    public ValorantPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await PopulateAsync().ConfigureAwait(true);
    }

    private async Task PopulateAsync()
    {
        var model = await GamePageLoader.LoadAsync(GameId.Valorant).ConfigureAwait(true);

        DetectionSlot.Content = GamePageLoader.BuildDetectionBadge(model.Installation);
        InstallPathSlot.Content = GamePageLoader.BuildInstallPathRow(model.Installation);
        LaunchOptionsText.Text = model.Recommendation.LaunchOptions;
        SettingsSlot.Content = GamePageLoader.BuildSettingsList(model.Recommendation.Settings);
        NotesSlot.Content = GamePageLoader.BuildNotesList(model.Recommendation.Notes);

        if (model.VanguardInstalled && model.VanguardBlockedTweaks.Count > 0)
        {
            VanguardWarningCard.Visibility = Visibility.Visible;
            BlockedSlot.Content = GamePageLoader.BuildBlockedTweaksList(model.VanguardBlockedTweaks);
        }
    }
}
