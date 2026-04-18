using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Controls;

public partial class TweakCard : UserControl
{
    public static readonly DependencyProperty MetadataProperty = DependencyProperty.Register(
        nameof(Metadata),
        typeof(TweakMetadata),
        typeof(TweakCard),
        new PropertyMetadata(null, OnMetadataChanged));

    public static readonly DependencyProperty IsAppliedProperty = DependencyProperty.Register(
        nameof(IsApplied),
        typeof(bool),
        typeof(TweakCard),
        new PropertyMetadata(false, OnIsAppliedChanged));

    public static readonly RoutedEvent ToggleRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(ToggleRequested),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TweakCard));

    public TweakCard()
    {
        InitializeComponent();
    }

    public TweakMetadata? Metadata
    {
        get => (TweakMetadata?)GetValue(MetadataProperty);
        set => SetValue(MetadataProperty, value);
    }

    public bool IsApplied
    {
        get => (bool)GetValue(IsAppliedProperty);
        set => SetValue(IsAppliedProperty, value);
    }

    public event RoutedEventHandler ToggleRequested
    {
        add => AddHandler(ToggleRequestedEvent, value);
        remove => RemoveHandler(ToggleRequestedEvent, value);
    }

    private static void OnMetadataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TweakCard card && e.NewValue is TweakMetadata metadata)
        {
            card.ApplyMetadata(metadata);
        }
    }

    private static void OnIsAppliedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TweakCard card && e.NewValue is bool applied)
        {
            card.ApplyToggle.IsOn = applied;
        }
    }

    private void ApplyMetadata(TweakMetadata metadata)
    {
        NameText.Text = metadata.Name;
        ShortDescriptionText.Text = metadata.ShortDescription;

        RiskText.Text = metadata.Risk switch
        {
            TweakRisk.Safe => "SAFE",
            TweakRisk.Medium => "MEDIUM",
            TweakRisk.High => "HIGH",
            _ => "UNKNOWN"
        };

        RiskBadge.Background = metadata.Risk switch
        {
            TweakRisk.Safe => new SolidColorBrush(Color.FromRgb(46, 125, 50)),
            TweakRisk.Medium => new SolidColorBrush(Color.FromRgb(191, 118, 32)),
            TweakRisk.High => new SolidColorBrush(Color.FromRgb(183, 28, 28)),
            _ => new SolidColorBrush(Color.FromRgb(97, 97, 97))
        };

        EvidenceText.Text = metadata.Evidence switch
        {
            TweakEvidenceLevel.Confirmed => "Confirmed",
            TweakEvidenceLevel.Controversial => "Controversial",
            TweakEvidenceLevel.Myth => "Myth",
            _ => "Unknown"
        };

        TooltipTitle.Text = metadata.Name;
        TooltipDetailed.Text = metadata.DetailedDescription;
        TooltipModifies.Text = metadata.Modifies;
        TooltipImpact.Text = metadata.ExpectedImpact;
        TooltipRisk.Text = $"{metadata.Risk} - {metadata.Evidence}";
        TooltipSources.ItemsSource = metadata.Sources;
    }

    private void ApplyToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (Metadata is null)
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(ToggleRequestedEvent, this));
    }
}
