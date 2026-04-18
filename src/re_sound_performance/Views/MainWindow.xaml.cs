using System.Windows;
using ModernWpf.Controls;
using re_sound_performance.Views.Pages;

namespace re_sound_performance.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RootNav.SelectedItem = RootNav.MenuItems[0];
        ContentFrame.Navigate(new HomePage());
    }

    private void RootNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(new SettingsPage());
            return;
        }

        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateByTag(tag);
        }
    }

    private void NavigateByTag(string tag)
    {
        System.Windows.Controls.Page page = tag switch
        {
            "home" => new HomePage(),
            "system" => new SystemTweaksPage(),
            "gpu" => new GpuPage(),
            "debloat" => new DebloatPage(),
            "privacy" => new PrivacyPage(),
            "games" => new GamesHubPage(),
            "game_cs2" => new Cs2Page(),
            "game_valorant" => new ValorantPage(),
            "game_apex" => new ApexPage(),
            "benchmark" => new BenchmarkPage(),
            "history" => new HistoryPage(),
            _ => new HomePage()
        };

        ContentFrame.Navigate(page);
    }
}
