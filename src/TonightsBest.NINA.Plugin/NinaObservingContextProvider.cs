using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
using NINA.Core.Model;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Profile.Interfaces;
using TonightsBest.Core;

namespace TonightsBest.NINA.Plugin;

internal sealed class NinaObservingContextProvider(
    IProfileService profileService,
    INighttimeCalculator nighttimeCalculator,
    Func<CameraInfo?> camera,
    Func<TelescopeInfo?> telescope,
    Func<double> minimumAltitude) : IObservingContextProvider {
    public Task<ObservingContext> GetAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var profile = profileService.ActiveProfile;
        var cameraInfo = camera() ?? throw new InvalidOperationException("Connect or select a camera before refreshing Tonight's Best.");
        var telescopeInfo = telescope();
        var focalLength = profile.TelescopeSettings.FocalLength;
        if (focalLength <= 0) throw new InvalidOperationException("Set the telescope focal length in the active N.I.N.A. profile.");
        var pixelSize = cameraInfo.PixelSize > 0 ? cameraInfo.PixelSize : profile.CameraSettings.PixelSize;
        if (!cameraInfo.Connected || cameraInfo.XSize <= 0 || cameraInfo.YSize <= 0 || pixelSize <= 0)
            throw new InvalidOperationException("Connect the selected camera so N.I.N.A. can report its sensor width, height, and pixel size.");
        var equipment = new EquipmentProfile(focalLength, cameraInfo.XSize, cameraInfo.YSize, pixelSize);

        var night = nighttimeCalculator.Calculate();
        var start = night.TwilightRiseAndSet.Set ?? night.SunRiseAndSet.Set ?? DateTime.Now;
        var end = night.TwilightRiseAndSet.Rise ?? night.SunRiseAndSet.Rise ?? start.AddHours(10);
        if (end <= start) end = end.AddDays(1);
        var observer = new ObserverInfo {
            Latitude = profile.AstrometrySettings.Latitude,
            Longitude = profile.AstrometrySettings.Longitude,
            Elevation = profile.AstrometrySettings.Elevation
        };
        var moonPosition = AstroUtil.GetMoonPosition(start.AddTicks((end - start).Ticks / 2), 0, observer);
        var moon = new MoonState(AstroUtil.HoursToDegrees(moonPosition.RA), moonPosition.Dec,
            Math.Clamp(night.Illumination ?? 0, 0, 1));
        return Task.FromResult(new ObservingContext(
            new DateTimeOffset(start), new DateTimeOffset(end), observer.Latitude, observer.Longitude,
            minimumAltitude(), equipment.FieldOfView, moon, TimeSpan.FromMinutes(5)));
    }
}
