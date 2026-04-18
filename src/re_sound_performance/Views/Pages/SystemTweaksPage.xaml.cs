using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class SystemTweaksPage : Page
{
    private static readonly TweakCategory[] AllowedCategories =
    {
        TweakCategory.System,
        TweakCategory.Input,
        TweakCategory.Power,
        TweakCategory.Network
    };

    private readonly TweakEngine _engine;

    public SystemTweaksPage()
    {
        InitializeComponent();
        _engine = ((App)Application.Current).Services.GetRequiredService<TweakEngine>();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) =>
        await TweakPageLoader.PopulateAsync(_engine, TweaksList, AllowedCategories).ConfigureAwait(true);
}
