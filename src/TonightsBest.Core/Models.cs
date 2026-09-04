namespace TonightsBest.Core;

public sealed record EquipmentProfile(
    double FocalLengthMillimeters,
    int CameraWidthPixels,
    int CameraHeightPixels,
    double PixelSizeMicrons) {
    public FieldOfView FieldOfView => FieldOfView.From(this);
}

public sealed record FieldOfView(double WidthArcMinutes, double HeightArcMinutes) {
    public static FieldOfView From(EquipmentProfile equipment) {
        if (equipment.FocalLengthMillimeters <= 0 || equipment.CameraWidthPixels <= 0 ||
            equipment.CameraHeightPixels <= 0 || equipment.PixelSizeMicrons <= 0) {
            throw new ArgumentOutOfRangeException(nameof(equipment), "Connected camera dimensions, pixel size, and telescope focal length must be positive.");
        }
        const double radiansToArcMinutes = 3437.7467707849396;
        var sensorWidth = equipment.CameraWidthPixels * equipment.PixelSizeMicrons / 1000d;
        var sensorHeight = equipment.CameraHeightPixels * equipment.PixelSizeMicrons / 1000d;
        return new(2 * Math.Atan(sensorWidth / (2 * equipment.FocalLengthMillimeters)) * radiansToArcMinutes,
                   2 * Math.Atan(sensorHeight / (2 * equipment.FocalLengthMillimeters)) * radiansToArcMinutes);
    }
}

public sealed record SkyTarget(
    string Id,
    string Name,
    string ObjectType,
    double RightAscensionDegrees,
    double DeclinationDegrees,
    double MajorAxisArcMinutes,
    double MinorAxisArcMinutes,
    double? Magnitude,
    string Constellation = "",
    string CatalogTypeCode = "");

public sealed record MoonState(double RightAscensionDegrees, double DeclinationDegrees, double IlluminatedFraction);

public sealed record ObservingContext(
    DateTimeOffset Start,
    DateTimeOffset End,
    double LatitudeDegrees,
    double LongitudeDegrees,
    double MinimumAltitudeDegrees,
    FieldOfView FieldOfView,
    MoonState Moon,
    TimeSpan SampleInterval);

public sealed record ScoreBreakdown(double Visibility, double Altitude, double Moon, double Framing, double ObjectInterest);

public sealed record RankedTarget(
    SkyTarget Target,
    double Score,
    double FrameCoveragePercent,
    double HoursAboveMinimumAltitude,
    double MaximumAltitudeDegrees,
    double MoonSeparationDegrees,
    ScoreBreakdown Breakdown);
