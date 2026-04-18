using System.Windows;
using re_sound_performance.Core.Games;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class Cs2Page : System.Windows.Controls.Page
{
    private GameInstallation? _installation;

    public Cs2Page()
    {
        InitializeComponent();
        Loaded += async (_, _) => await PopulateAsync().ConfigureAwait(true);
    }

    private async Task PopulateAsync()
    {
        var model = await GamePageLoader.LoadAsync(GameId.Cs2).ConfigureAwait(true);
        _installation = model.Installation;

        DetectionSlot.Content = GamePageLoader.BuildDetectionBadge(model.Installation);
        InstallPathSlot.Content = GamePageLoader.BuildInstallPathRow(model.Installation);
        LaunchOptionsBox.Text = model.Recommendation.LaunchOptions;
        AutoexecBox.Text = model.Recommendation.ConfigFileContent ?? string.Empty;
        SettingsSlot.Content = GamePageLoader.BuildSettingsList(model.Recommendation.Settings);
        NotesSlot.Content = GamePageLoader.BuildNotesList(model.Recommendation.Notes);
        WriteAutoexecButton.IsEnabled = model.Installation.Installed;
    }

    private void CopyLaunch_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(LaunchOptionsBox.Text);
    }

    private async void WriteAutoexec_Click(object sender, RoutedEventArgs e)
    {
        if (_installation is null)
        {
            return;
        }

        WriteAutoexecButton.IsEnabled = false;
        WriteResultText.Text = "Writing autoexec.cfg...";

        try
        {
            var result = await GamePageLoader.WriteRecommendedAsync(_installation, GameRecommendations.Cs2).ConfigureAwait(true);
            WriteResultText.Text = result.Outcome switch
            {
                GameConfigWriteOutcome.Written => result.BackupPath is null
                    ? $"Wrote autoexec.cfg."
                    : $"Wrote autoexec.cfg (previous saved as {System.IO.Path.GetFileName(result.BackupPath)}).",
                GameConfigWriteOutcome.MissingInstall => "CS2 install not detected.",
                GameConfigWriteOutcome.Skipped => "No config file to write.",
                _ => result.Message ?? "Write failed."
            };
        }
        finally
        {
            WriteAutoexecButton.IsEnabled = _installation.Installed;
        }
    }
}
