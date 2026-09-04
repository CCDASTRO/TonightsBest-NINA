using TonightsBest.Core;
using Xunit;

namespace TonightsBest.Core.Tests;

public class ScoringTests {
    [Fact] public void FieldOfViewUsesSelectedOptics() {
        var fov = new EquipmentProfile(500, 6000, 4000, 3.76).FieldOfView;
        Assert.InRange(fov.WidthArcMinutes, 154.9, 155.2);
        Assert.InRange(fov.HeightArcMinutes, 103.3, 103.5);
    }

    [Fact] public void MoonPenaltyIncreasesWhenBrightMoonIsClose() {
        var scorer = new TargetScorer();
        var target = new SkyTarget("M42", "M 42", "Emission Nebula", 83.8, -5.4, 65, 60, 4);
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var common = new ObservingContext(start, start.AddHours(8), 35, -80, -90, new FieldOfView(120, 80), new MoonState(84, -5, 1), TimeSpan.FromMinutes(5));
        var near = scorer.Score(target, common);
        var far = scorer.Score(target, common with { Moon = new MoonState(264, 5, 1) });
        Assert.True(far.Score > near.Score);
        Assert.True(far.MoonSeparationDegrees > near.MoonSeparationDegrees);
    }

    [Fact] public void RankReturnsOnlyRequestedVisibleTargets() {
        var scorer = new TargetScorer();
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var context = new ObservingContext(start, start.AddHours(8), 35, -80, -90, new FieldOfView(120, 80), new MoonState(0, 0, .2), TimeSpan.FromMinutes(10));
        var targets = Enumerable.Range(0, 25).Select(i => new SkyTarget(i.ToString(), $"Target {i}", "Galaxy", i * 14, 20, 30, 20, 10));
        var ranked = scorer.Rank(targets, context, 15);
        Assert.Equal(15, ranked.Count);
        Assert.True(ranked.Zip(ranked.Skip(1)).All(pair => pair.First.Score >= pair.Second.Score));
    }

    [Fact] public void OversizedTargetReportsMoreThanOneHundredPercentCoverage() {
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var context = new ObservingContext(start, start.AddHours(4), 35, -80, -90,
            new FieldOfView(60, 40), new MoonState(180, 0, 0), TimeSpan.FromMinutes(10));
        var result = new TargetScorer().Score(new SkyTarget("X", "Large", "Nebula", 0, 30, 120, 80, null), context);
        Assert.True(result.FrameCoveragePercent > 100);
        Assert.InRange(result.Score, 0, 100);
        Assert.Equal(result.Score, Math.Round(result.Breakdown.Visibility + result.Breakdown.Altitude +
            result.Breakdown.Moon + result.Breakdown.Framing + result.Breakdown.ObjectInterest, 1));
    }
}
