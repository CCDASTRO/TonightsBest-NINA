# Architecture notes

## Boundaries

The core project never references WPF or N.I.N.A. SDK assemblies. Three ports
define the host boundary: `ISkyAtlasCatalog`, `IObservingContextProvider`, and
`IFramingAssistantGateway`.

The plugin adapter translates N.I.N.A. `DeepSkyObject` records to
`SkyTarget`, snapshot the active profile and connected equipment into an
`ObservingContext`, and translate the selected target back for
`IFramingAssistantVM.SetCoordinates`. The catalog query is intentionally
unlimited: ranking all returned records avoids a hidden bias toward large
objects and still returns only the best 15 to the UI.
Both the localized display type and N.I.N.A.'s original catalog type code are
retained; the friendly label is shown in the grid, while the native code is sent
back to Framing Assistant.

## Time and coordinates

All engine times are `DateTimeOffset`; longitude is positive east. Right
ascension is stored in degrees and catalog angular dimensions in arcminutes.
Altitude is sampled across the requested observing window rather than inferred
from transit alone.

## Safety and responsiveness

Atlas searches and refreshes are cancellable. The WPF adapter must not block the
UI thread, must disregard stale refresh results, and must show missing equipment
or location data instead of silently substituting defaults.

## Future conditions

Cloud cover, astronomical darkness, horizon masks, meridian limits, and target
priority can be added as separate score contributors. Weather should remain
optional: absence of a connected weather source must not prevent target ranking.

## Local Sky Atlas smoke test

The optional `tools/TonightsBest.AtlasSmokeTest` executable queries the installed
N.I.N.A. database through the same adapter as the plugin. It is intentionally not
part of CI because a GitHub runner has no `%LOCALAPPDATA%\NINA\NINA.sqlite`.
