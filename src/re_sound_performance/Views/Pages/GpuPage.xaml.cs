using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Views.Pages.Shared;

namespace re_sound_performance.Views.Pages;

public partial class GpuPage : Page
{
    private readonly TweakEngine _engine;

    public GpuPage()
    {
        InitializeComponent();
        _engine = ((App)Application.Current).Services.GetRequiredService<TweakEngine>();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) =>
        await TweakPageLoader.PopulateAsync(_engine, TweaksList, new[] { TweakCategory.Gpu }).ConfigureAwait(true);
}
