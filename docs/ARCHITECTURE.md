# Architecture notes

## Boundaries

The core project never references WPF or N.I.N.A. SDK assemblies. Three ports
define the host boundary: `ISkyAtlasCatalog`, `IObservingContextProvider`, and
`IFramingAssistantGateway`.

The plugin adapter will translate N.I.N.A. `DeepSkyObject` records to
`SkyTarget`, snapshot the active profile and connected equipment into an
`ObservingContext`, and translate the selected target back for
`IFramingAssistantVM.SetCoordinates`.

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
