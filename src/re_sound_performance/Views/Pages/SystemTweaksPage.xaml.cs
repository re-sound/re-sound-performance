using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using re_sound_performance.Controls;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Views.Pages;

public partial class SystemTweaksPage : Page
{
    private readonly TweakEngine _engine;

    public SystemTweaksPage()
    {
        InitializeComponent();
        _engine = ((App)Application.Current).Services.GetRequiredService<TweakEngine>();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TweaksList.Items.Clear();
        foreach (var tweak in _engine.AvailableTweaks)
        {
            if (tweak.Metadata.Category is not TweakCategory.System and not TweakCategory.Input)
            {
                continue;
            }

            var status = await _engine.ProbeAsync(tweak.Metadata.Id).ConfigureAwait(true);

            var card = new TweakCard
            {
                Metadata = tweak.Metadata,
                IsApplied = status == TweakStatus.Applied
            };

            card.ToggleRequested += async (s, args) => await OnToggleRequested(card, tweak);

            TweaksList.Items.Add(card);
        }
    }

    private async Task OnToggleRequested(TweakCard card, ITweak tweak)
    {
        if (card.IsApplied)
        {
            var result = await _engine.RevertAsync(tweak.Metadata.Id).ConfigureAwait(true);
            card.IsApplied = result.ResultingStatus == TweakStatus.Applied;
        }
        else
        {
            var result = await _engine.ApplyAsync(tweak.Metadata.Id).ConfigureAwait(true);
            card.IsApplied = result.ResultingStatus == TweakStatus.Applied;
        }
    }
}
