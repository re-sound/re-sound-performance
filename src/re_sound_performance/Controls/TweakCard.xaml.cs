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

        RiskBadge.Background = ResourceBrush(metadata.Risk switch
        {
            TweakRisk.Safe => "ReSound.Risk.Safe",
            TweakRisk.Medium => "ReSound.Risk.Medium",
            TweakRisk.High => "ReSound.Risk.High",
            _ => "ReSound.Surface.Muted"
        });

        EvidenceText.Text = metadata.Evidence switch
        {
            TweakEvidenceLevel.Confirmed => "Confirmed",
            TweakEvidenceLevel.Controversial => "Controversial",
            TweakEvidenceLevel.Myth => "Myth",
            _ => "Unknown"
        };

        var accent = CategoryColor(metadata.Category);
        AccentStop1.Color = accent;
        AccentStop2.Color = Darken(accent, 0.45);

        TooltipTitle.Text = metadata.Name;
        TooltipDetailed.Text = metadata.DetailedDescription;
        TooltipModifies.Text = metadata.Modifies;
        TooltipImpact.Text = metadata.ExpectedImpact;
        TooltipRisk.Text = $"{metadata.Risk} - {metadata.Evidence}";
        TooltipSources.ItemsSource = metadata.Sources;
    }

    private static Color CategoryColor(TweakCategory category)
    {
        var key = category switch
        {
            TweakCategory.System => "ReSound.Color.Category.System",
            TweakCategory.Privacy => "ReSound.Color.Category.Privacy",
            TweakCategory.Debloat => "ReSound.Color.Category.Debloat",
            TweakCategory.Gpu => "ReSound.Color.Category.Gpu",
            TweakCategory.Network => "ReSound.Color.Category.Network",
            TweakCategory.Input => "ReSound.Color.Category.Input",
            TweakCategory.Power => "ReSound.Color.Category.Power",
            TweakCategory.Game => "ReSound.Color.Category.Game",
            _ => "ReSound.Color.Accent"
        };

        if (Application.Current?.TryFindResource(key) is Color color)
        {
            return color;
        }

        return Colors.MediumPurple;
    }

    private static Color Darken(Color color, double factor)
    {
        factor = Math.Clamp(factor, 0, 1);
        var scale = 1.0 - factor;
        return Color.FromArgb(
            color.A,
            (byte)(color.R * scale),
            (byte)(color.G * scale),
            (byte)(color.B * scale));
    }

    private static Brush ResourceBrush(string key)
    {
        if (Application.Current?.TryFindResource(key) is Brush brush)
        {
            return brush;
        }

        return Brushes.Gray;
    }

    private void RenderStatus(TweakCardStatus status)
    {
        if (status == TweakCardStatus.Idle)
        {
            StatusPill.Visibility = Visibility.Collapsed;
            return;
        }

        StatusPill.Visibility = Visibility.Visible;

        var (icon, label, brushKey) = status switch
        {
            TweakCardStatus.Applying => ("...", "Applying", "ReSound.Status.Info"),
            TweakCardStatus.Reverting => ("...", "Reverting", "ReSound.Status.Info"),
            TweakCardStatus.Success => ("\u2713", "OK", "ReSound.Status.Success"),
            TweakCardStatus.Failed => ("\u2715", "Error", "ReSound.Status.Danger"),
            TweakCardStatus.Partial => ("\u25B2", "Partial", "ReSound.Status.Warning"),
            TweakCardStatus.Unavailable => ("\u2013", "N/A", "ReSound.Surface.Muted"),
            _ => (string.Empty, string.Empty, "ReSound.Surface.Muted")
        };

        StatusIcon.Text = icon;
        StatusText.Text = label;
        StatusPill.Background = ResourceBrush(brushKey);

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
