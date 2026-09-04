using NINA.Astrometry;
using NINA.Core.Locale;
using NINA.Profile.Interfaces;
using TonightsBest.Core;

namespace TonightsBest.NINA.Plugin;

internal sealed class NinaSkyAtlasCatalog(IProfileService profileService) : ISkyAtlasCatalog {
    public async Task<IReadOnlyList<SkyTarget>> SearchAsync(CancellationToken cancellationToken) {
        var profile = profileService.ActiveProfile;
        var search = new DatabaseInteraction.DeepSkyObjectSearchParams {
            DsoTypes = Array.Empty<string>(),
            Limit = 1500,
            SearchOrder = new DatabaseInteraction.DeepSkyObjectSearchOrder { Field = "sizemax", Direction = "DESC" }
        };
        var objects = await new DatabaseInteraction().GetDeepSkyObjects(
            imageFactory: null!, profile.AstrometrySettings.Horizon, search, cancellationToken).ConfigureAwait(false);
        return objects.Select(Map).ToArray();
    }

    private static SkyTarget Map(DeepSkyObject value) => new(
        value.Id, value.Name, FriendlyObjectType(value.DSOType), value.Coordinates.RADegrees,
        value.Coordinates.Dec, (value.Size ?? 0) / 60d, (value.SizeMin ?? value.Size ?? 0) / 60d,
        value.Magnitude, value.Constellation ?? string.Empty);

    private static string FriendlyObjectType(string? code) {
        if (string.IsNullOrWhiteSpace(code)) return "Other";
        var label = Loc.Instance[$"LblObjectType_{code}"];
        return string.IsNullOrWhiteSpace(label) || label.StartsWith("LblObjectType_", StringComparison.Ordinal)
            ? code
            : label;
    }
}
