namespace TonightsBest.Core;

public static class AstronomyMath {
    public static double AltitudeDegrees(double raDegrees, double decDegrees, DateTimeOffset instant, double latitudeDegrees, double longitudeDegrees) {
        var hourAngle = NormalizeSigned(LocalSiderealDegrees(instant, longitudeDegrees) - raDegrees);
        var lat = ToRadians(latitudeDegrees);
        var dec = ToRadians(decDegrees);
        var altitude = Math.Asin(Math.Sin(lat) * Math.Sin(dec) + Math.Cos(lat) * Math.Cos(dec) * Math.Cos(ToRadians(hourAngle)));
        return ToDegrees(altitude);
    }

    public static double AngularSeparationDegrees(double ra1, double dec1, double ra2, double dec2) {
        var d1 = ToRadians(dec1); var d2 = ToRadians(dec2);
        var deltaRa = ToRadians(ra1 - ra2);
        var cosine = Math.Sin(d1) * Math.Sin(d2) + Math.Cos(d1) * Math.Cos(d2) * Math.Cos(deltaRa);
        return ToDegrees(Math.Acos(Math.Clamp(cosine, -1d, 1d)));
    }

    public static double LocalSiderealDegrees(DateTimeOffset instant, double longitudeDegrees) {
        var utc = instant.UtcDateTime;
        var jd = utc.ToOADate() + 2415018.5;
        var t = (jd - 2451545d) / 36525d;
        var gmst = 280.46061837 + 360.98564736629 * (jd - 2451545d) + 0.000387933 * t * t - t * t * t / 38710000d;
        return Normalize(gmst + longitudeDegrees);
    }

    private static double Normalize(double value) => ((value % 360d) + 360d) % 360d;
    private static double NormalizeSigned(double value) { var n = Normalize(value); return n > 180 ? n - 360 : n; }
    private static double ToRadians(double value) => value * Math.PI / 180d;
    private static double ToDegrees(double value) => value * 180d / Math.PI;
}

