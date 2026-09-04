using NINA.Astrometry;
using NINA.Core.Locale;
using NINA.Profile.Interfaces;
using TonightsBest.Core;

namespace TonightsBest.NINA.Plugin;

internal sealed class NinaSkyAtlasCatalog : ISkyAtlasCatalog {
    private readonly Func<global::NINA.Core.Model.CustomHorizon> horizon;
    private readonly Func<DatabaseInteraction> database;

    public NinaSkyAtlasCatalog(IProfileService profileService) : this(
        () => profileService.ActiveProfile.AstrometrySettings.Horizon,
        () => new DatabaseInteraction()) { }

    internal NinaSkyAtlasCatalog(
        Func<global::NINA.Core.Model.CustomHorizon> horizon,
        Func<DatabaseInteraction> database) {
        this.horizon = horizon;
        this.database = database;
    }

    public async Task<IReadOnlyList<SkyTarget>> SearchAsync(CancellationToken cancellationToken) {
        var search = new DatabaseInteraction.DeepSkyObjectSearchParams {
            DsoTypes = Array.Empty<string>()
        };
        var objects = await database().GetDeepSkyObjects(
            imageFactory: null!, horizon(), search, cancellationToken).ConfigureAwait(false);
        return objects.Select(Map).ToArray();
    }

    private static SkyTarget Map(DeepSkyObject value) => new(
        value.Id, value.Name, FriendlyObjectType(value.DSOType), value.Coordinates.RADegrees,
        value.Coordinates.Dec, (value.Size ?? 0) / 60d, (value.SizeMin ?? value.Size ?? 0) / 60d,
        value.Magnitude, value.Constellation ?? string.Empty, value.DSOType ?? string.Empty);

    private static string FriendlyObjectType(string? code) {
        if (string.IsNullOrWhiteSpace(code)) return "Other";
        var label = Loc.Instance[$"LblObjectType_{code}"];
        return string.IsNullOrWhiteSpace(label) || label.StartsWith("LblObjectType_", StringComparison.Ordinal)
            ? code
            : label;
    }
}
