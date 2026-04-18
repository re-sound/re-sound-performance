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

    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status),
        typeof(TweakCardStatus),
        typeof(TweakCard),
        new PropertyMetadata(TweakCardStatus.Idle, OnStatusChanged));

    public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register(
        nameof(StatusMessage),
        typeof(string),
        typeof(TweakCard),
        new PropertyMetadata(null, OnStatusMessageChanged));

    public static readonly RoutedEvent ToggleRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(ToggleRequested),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TweakCard));

    private bool _suppressToggle;

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

    public TweakCardStatus Status
    {
        get => (TweakCardStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string? StatusMessage
    {
        get => (string?)GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    public event RoutedEventHandler ToggleRequested
    {
        add => AddHandler(ToggleRequestedEvent, value);
        remove => RemoveHandler(ToggleRequestedEvent, value);
    }

    public void SetAppliedSilently(bool value)
    {
        _suppressToggle = true;
        try
        {
            IsApplied = value;
        }
        finally
        {
            _suppressToggle = false;
        }
    }

    public void SetToggleEnabled(bool enabled) => ApplyToggle.IsEnabled = enabled;

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

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TweakCard card && e.NewValue is TweakCardStatus status)
        {
            card.RenderStatus(status);
        }
    }

    private static void OnStatusMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TweakCard card)
        {
            card.RefreshStatusTooltip();
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

        AccentStripe.Background = CategoryAccent(metadata.Category);

        TooltipTitle.Text = metadata.Name;
        TooltipDetailed.Text = metadata.DetailedDescription;
        TooltipModifies.Text = metadata.Modifies;
        TooltipImpact.Text = metadata.ExpectedImpact;
        TooltipRisk.Text = $"{metadata.Risk} - {metadata.Evidence}";
        TooltipSources.ItemsSource = metadata.Sources;
    }

    private static SolidColorBrush CategoryAccent(TweakCategory category) => category switch
    {
        TweakCategory.System => new SolidColorBrush(Color.FromRgb(0x0A, 0xA2, 0xA2)),
        TweakCategory.Privacy => new SolidColorBrush(Color.FromRgb(0x8E, 0x44, 0xE0)),
        TweakCategory.Debloat => new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)),
        TweakCategory.Gpu => new SolidColorBrush(Color.FromRgb(0x7C, 0xB3, 0x42)),
        TweakCategory.Network => new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4)),
        TweakCategory.Input => new SolidColorBrush(Color.FromRgb(0xEC, 0x40, 0x7A)),
        TweakCategory.Power => new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x00)),
        TweakCategory.Game => new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)),
        _ => new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61))
    };

    private void RenderStatus(TweakCardStatus status)
    {
        if (status == TweakCardStatus.Idle)
        {
            StatusPill.Visibility = Visibility.Collapsed;
            return;
        }

        StatusPill.Visibility = Visibility.Visible;

        var (icon, label, brush) = status switch
        {
            TweakCardStatus.Applying => ("...", "Applying", new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5))),
            TweakCardStatus.Reverting => ("...", "Reverting", new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5))),
            TweakCardStatus.Success => ("\u2713", "OK", new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))),
            TweakCardStatus.Failed => ("\u2715", "Error", new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C))),
            TweakCardStatus.Partial => ("\u25B2", "Partial", new SolidColorBrush(Color.FromRgb(0xBF, 0x76, 0x20))),
            TweakCardStatus.Unavailable => ("\u2013", "N/A", new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61))),
            _ => (string.Empty, string.Empty, new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)))
        };

        StatusIcon.Text = icon;
        StatusText.Text = label;
        StatusPill.Background = brush;

        RefreshStatusTooltip();
    }

    private void RefreshStatusTooltip()
    {
        var message = StatusMessage;
        if (string.IsNullOrWhiteSpace(message))
        {
            StatusPill.ToolTip = Status switch
            {
                TweakCardStatus.Success => "Verified: the tweak is active on disk.",
                TweakCardStatus.Failed => "The tweak did not apply. Check logs or hover ? for details.",
                TweakCardStatus.Partial => "Some of the changes did not take effect.",
                TweakCardStatus.Unavailable => "This tweak does not apply to the current system.",
                TweakCardStatus.Applying => "Applying the tweak...",
                TweakCardStatus.Reverting => "Reverting the tweak...",
                _ => null
            };
        }
        else
        {
            StatusPill.ToolTip = message;
        }
    }

    private void ApplyToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_suppressToggle)
        {
            return;
        }

        if (Metadata is null)
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(ToggleRequestedEvent, this));
    }
}
