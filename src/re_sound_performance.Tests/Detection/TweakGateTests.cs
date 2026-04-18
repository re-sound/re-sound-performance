using FluentAssertions;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Tests.Detection;

public sealed class TweakGateTests
{
    private static TweakMetadata Metadata(bool blockedVanguard = false, bool blockedFaceit = false) => new(
        "test.sample",
        "Sample",
        "short",
        "detailed",
        "modifies",
        "impact",
        TweakCategory.System,
        TweakRisk.Safe,
        TweakEvidenceLevel.Confirmed,
        Array.Empty<string>(),
        Array.Empty<string>(),
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: blockedVanguard,
        BlockedWhenFaceitInstalled: blockedFaceit);

    [Fact]
    public void Evaluate_NullContext_Allows()
    {
        var decision = TweakGate.Evaluate(Metadata(blockedVanguard: true), antiCheat: null);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NoVanguard_AllowsVanguardBlocked()
    {
        var decision = TweakGate.Evaluate(Metadata(blockedVanguard: true), AntiCheatInfo.None);

        decision.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_VanguardInstalledBlocksMatchingTweak()
    {
        var ac = new AntiCheatInfo(AntiCheat.Vanguard, new[] { "Vanguard" });

        var decision = TweakGate.Evaluate(Metadata(blockedVanguard: true), ac);

        decision.Allowed.Should().BeFalse();
        decision.Reason.Should().Contain("Vanguard");
    }

    [Fact]
    public void Evaluate_FaceitInstalledBlocksMatchingTweak()
    {
        var ac = new AntiCheatInfo(AntiCheat.FaceitAc, new[] { "FACEIT AC" });

        var decision = TweakGate.Evaluate(Metadata(blockedFaceit: true), ac);

        decision.Allowed.Should().BeFalse();
        decision.Reason.Should().Contain("FACEIT");
    }

    [Fact]
    public void Evaluate_InstalledButTweakNotMarkedBlocked_Allows()
    {
        var ac = new AntiCheatInfo(AntiCheat.Vanguard, new[] { "Vanguard" });

        var decision = TweakGate.Evaluate(Metadata(blockedVanguard: false), ac);

        decision.Allowed.Should().BeTrue();
    }
}
