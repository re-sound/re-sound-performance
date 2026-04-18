using FluentAssertions;
using re_sound_performance.Core.Presets;

namespace re_sound_performance.Tests.Presets;

public sealed class PresetCatalogTests
{
    [Fact]
    public void All_ReturnsThreePresets()
    {
        PresetCatalog.All.Should().HaveCount(3);
        PresetCatalog.All.Select(p => p.Kind).Should().BeEquivalentTo(new[]
        {
            PresetKind.Safe,
            PresetKind.Balanced,
            PresetKind.Competitive
        });
    }

    [Fact]
    public void BalancedIsSupersetOfSafe()
    {
        var safe = PresetCatalog.Get(PresetKind.Safe).TweakIds;
        var balanced = PresetCatalog.Get(PresetKind.Balanced).TweakIds;

        balanced.Should().Contain(safe.Intersect(balanced));
        balanced.Count.Should().BeGreaterThan(safe.Count);
    }

    [Fact]
    public void CompetitiveIsSupersetOfBalanced()
    {
        var balanced = PresetCatalog.Get(PresetKind.Balanced).TweakIds;
        var competitive = PresetCatalog.Get(PresetKind.Competitive).TweakIds;

        competitive.Should().Contain(balanced);
        competitive.Count.Should().BeGreaterThan(balanced.Count);
    }

    [Fact]
    public void NoPresetHasDuplicateTweakIds()
    {
        foreach (var preset in PresetCatalog.All)
        {
            preset.TweakIds.Should().OnlyHaveUniqueItems(because: $"{preset.Kind} should not repeat ids");
        }
    }
}
