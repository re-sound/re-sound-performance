using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class DebloatPage : Page
{
    private readonly TweakEngine _engine;

    public DebloatPage()
    {
        InitializeComponent();
        _engine = ((App)Application.Current).Services.GetRequiredService<TweakEngine>();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) =>
        await TweakPageLoader.PopulateAsync(_engine, TweaksList, new[] { TweakCategory.Debloat }).ConfigureAwait(true);
}
