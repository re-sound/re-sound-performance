using System.Windows;
using re_sound_performance.Core.Games;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class ApexPage : System.Windows.Controls.Page
{
    public ApexPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await PopulateAsync().ConfigureAwait(true);
    }

    private async Task PopulateAsync()
    {
        var model = await GamePageLoader.LoadAsync(GameId.Apex).ConfigureAwait(true);

        DetectionSlot.Content = GamePageLoader.BuildDetectionBadge(model.Installation);
        InstallPathSlot.Content = GamePageLoader.BuildInstallPathRow(model.Installation);
        LaunchOptionsBox.Text = model.Recommendation.LaunchOptions;
        SettingsSlot.Content = GamePageLoader.BuildSettingsList(model.Recommendation.Settings);
        NotesSlot.Content = GamePageLoader.BuildNotesList(model.Recommendation.Notes);
    }

    private void CopyLaunch_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(LaunchOptionsBox.Text);
    }
}
