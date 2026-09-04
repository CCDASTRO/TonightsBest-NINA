using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using TonightsBest.Core;

namespace TonightsBest.NINA.Plugin;

internal sealed class NinaFramingAssistantGateway(
    IProfileService profileService,
    IFramingAssistantVM framingAssistant,
    IApplicationMediator applicationMediator) : IFramingAssistantGateway {
    public async Task OpenAsync(SkyTarget target, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var coordinates = new Coordinates(target.RightAscensionDegrees, target.DeclinationDegrees, Epoch.J2000, Coordinates.RAType.Degrees);
        var dso = new DeepSkyObject(target.Id, coordinates, profileService.ActiveProfile.AstrometrySettings.Horizon) {
            Name = target.Name,
            DSOType = target.ObjectType,
            Magnitude = target.Magnitude,
            Size = target.MajorAxisArcMinutes * 60,
            SizeMin = target.MinorAxisArcMinutes * 60,
            Constellation = target.Constellation
        };
        applicationMediator.ChangeTab(ApplicationTab.FRAMINGASSISTANT);
        await framingAssistant.SetCoordinates(dso);
    }
}
