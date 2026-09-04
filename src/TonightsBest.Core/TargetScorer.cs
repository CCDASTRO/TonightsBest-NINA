namespace TonightsBest.Core;

public sealed class TargetScorer {
    public IReadOnlyList<RankedTarget> Rank(IEnumerable<SkyTarget> targets, ObservingContext context, int count = 15) {
        ArgumentNullException.ThrowIfNull(targets);
        if (context.End <= context.Start) throw new ArgumentException("The observing window must end after it starts.", nameof(context));
        if (context.SampleInterval <= TimeSpan.Zero) throw new ArgumentException("Sample interval must be positive.", nameof(context));
        return targets.Select(target => Score(target, context))
            .Where(target => target.HoursAboveMinimumAltitude > 0)
            .OrderByDescending(target => target.Score)
            .ThenByDescending(target => target.HoursAboveMinimumAltitude)
            .ThenBy(target => target.Target.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(0, count)).ToArray();
    }

    public RankedTarget Score(SkyTarget target, ObservingContext context) {
        var samples = SampleAltitudes(target, context).ToArray();
        var qualifying = samples.Count(a => a >= context.MinimumAltitudeDegrees);
        var totalHours = (context.End - context.Start).TotalHours;
        var hours = totalHours * qualifying / samples.Length;
        var maxAltitude = samples.Max();
        var moonSeparation = AstronomyMath.AngularSeparationDegrees(target.RightAscensionDegrees, target.DeclinationDegrees,
            context.Moon.RightAscensionDegrees, context.Moon.DeclinationDegrees);
        var coverage = CalculateCoverage(target, context.FieldOfView);

        var visibility = 35 * Math.Clamp(hours / Math.Min(6d, totalHours), 0d, 1d);
        var altitude = 10 * Math.Clamp((maxAltitude - context.MinimumAltitudeDegrees) / Math.Max(1d, 90 - context.MinimumAltitudeDegrees), 0d, 1d);
        var moonRisk = context.Moon.IlluminatedFraction * Math.Clamp((75 - moonSeparation) / 60d, 0d, 1d);
        var moon = 25 * (1 - moonRisk);
        var framing = 25 * FramingUtility(coverage);
        var interest = 5 * ObjectInterest(target.ObjectType);
        var breakdown = new ScoreBreakdown(visibility, altitude, moon, framing, interest);
        return new(target, Math.Round(visibility + altitude + moon + framing + interest, 1), Math.Round(coverage, 1),
            Math.Round(hours, 2), Math.Round(maxAltitude, 1), Math.Round(moonSeparation, 1), breakdown);
    }

    private static IEnumerable<double> SampleAltitudes(SkyTarget target, ObservingContext context) {
        for (var time = context.Start; time <= context.End; time += context.SampleInterval)
            yield return AstronomyMath.AltitudeDegrees(target.RightAscensionDegrees, target.DeclinationDegrees, time,
                context.LatitudeDegrees, context.LongitudeDegrees);
    }

    private static double CalculateCoverage(SkyTarget target, FieldOfView fov) {
        if (target.MajorAxisArcMinutes <= 0 || target.MinorAxisArcMinutes <= 0) return 0;
        var targetArea = Math.PI * target.MajorAxisArcMinutes * target.MinorAxisArcMinutes / 4d;
        return 100 * targetArea / (fov.WidthArcMinutes * fov.HeightArcMinutes);
    }

    private static double FramingUtility(double coverage) => coverage switch {
        <= 0 => 0.15,
        < 10 => 0.15 + coverage * 0.035,
        <= 75 => 0.5 + (coverage - 10) / 65 * 0.5,
        <= 100 => 1 - (coverage - 75) / 25 * 0.35,
        <= 200 => 0.65 - (coverage - 100) / 100 * 0.65,
        _ => 0
    };

    private static double ObjectInterest(string type) {
        var value = type.ToLowerInvariant();
        if (value.Contains("galaxy") || value.Contains("nebula") || value.Contains("cluster")) return 1;
        return 0.7;
    }
}
