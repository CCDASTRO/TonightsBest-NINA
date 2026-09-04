# Tonight's Best for N.I.N.A.

Tonight's Best is a N.I.N.A. 3.2 plugin that ranks deep-sky targets for
the current night using the active N.I.N.A. profile, equipment, sky atlas, Moon,
and visibility window. The goal is a dockable **Top 15** panel with one-click
handoff to the Framing Assistant and then the Advanced Sequencer.

## Status

This repository contains a buildable **0.1.0 development preview** with the
scoring engine, live N.I.N.A. 3.2 adapters, dockable panel, and Framing Assistant
handoff. It has been compiled, unit tested, queried against an installed N.I.N.A.
Sky Atlas, and loaded in N.I.N.A. A final test with a connected camera and the
user's active telescope profile is required before the first public release.

## User workflow

1. Open the **Tonight's Best** dockable panel in N.I.N.A.
2. Confirm the observing window and minimum altitude inherited from the active profile.
3. Click **Refresh Top 15**.
4. Compare score, object type, magnitude, frame coverage, hours above minimum
   altitude, maximum altitude, and Moon separation.
5. Select a target and click **Open in Framing Assistant**.
6. Adjust rotation or mosaic framing and send it to the Advanced Sequencer with
   N.I.N.A.'s normal controls.

## Scoring model (initial)

The score is deliberately explainable and shown as a breakdown:

- visibility above the minimum altitude: 35 points;
- Moon clearance adjusted by illuminated fraction: 25 points;
- framing suitability: 25 points;
- maximum altitude: 10 points;
- broad object-interest prior: 5 points.

Frame coverage is the catalog object's elliptical area divided by the camera's
rectangular field area. Values over 100% indicate that the catalog footprint is
larger than the field. It is an estimate—not a promise that all faint extensions
fit. Objects with missing catalog dimensions remain eligible but receive a
low-confidence framing score.

## Architecture

```text
N.I.N.A. dockable panel
        |
TonightBestService (use case)
   |          |             |
Sky Atlas   Active profile  Framing Assistant
adapter     and equipment   gateway
        \      |           /
          TargetScorer
      (pure, testable .NET)
```

- `TonightsBest.Core` contains immutable models, field-of-view math, altitude
  sampling, angular Moon separation, ranking, and ports.
- `TonightsBest.NINA.Plugin` contains only MEF/WPF and
  SDK adapters. This isolates N.I.N.A. API changes from ranking logic.
- Tests use no N.I.N.A. installation or hardware.

## SDK decisions

- Target stable `NINA.Plugin` **3.2.0.9001** (`net8.0-windows`).
- Export one `IPluginManifest` and one `IDockableVM` through MEF.
- Read the active location and configured telescope/camera from N.I.N.A. services.
- Query N.I.N.A.'s sky atlas rather than ship another catalog.
- Populate N.I.N.A.'s Framing Assistant with the selected `DeepSkyObject` rather
  than implement a second framing or sequencing UI.

The atlas adapter deliberately does not impose a size-sorted result limit. Every
object returned by the active N.I.N.A. Sky Atlas query remains eligible for
visibility and framing scoring, so compact galaxies and planetary nebulae are
not displaced by large catalog footprints.

## Build and test

Prerequisite: .NET 8 SDK.

```powershell
dotnet test TonightsBest.NINA.sln --configuration Release
```

Create an installable development ZIP and manifest with:

```powershell
.\packaging\build-package.ps1
```

See [installation instructions](docs/INSTALLATION.md) and the
[scoring reference](docs/SCORING.md). The
[SDK review](docs/SDK-REVIEW.md) records the official interfaces and plugin
patterns used by this implementation.

Pushing a four-part version tag such as `v0.1.0.0` runs the release workflow,
rebuilds and tests the solution, and publishes the installable ZIP plus its
generated manifest as GitHub release assets.

## Roadmap

- [x] SDK and plugin-pattern review
- [x] Domain model, field-of-view calculation, visibility and Moon scoring
- [x] Ranking service and unit tests
- [x] N.I.N.A. sky-atlas, profile, camera and telescope adapters
- [x] Dockable Top 15 WPF panel
- [x] Framing Assistant handoff
- [x] Reproducible development ZIP and manifest generation
- [ ] Hardware-backed N.I.N.A. integration test
- [ ] First GitHub release and official plugin-manifest submission

## License

MIT. N.I.N.A. itself and its SDK packages retain their own licenses.
