# Tonight's Best for N.I.N.A.

Tonight's Best is a N.I.N.A. 3.2 plugin that ranks deep-sky targets for
the current night using the active N.I.N.A. profile, equipment, sky atlas, Moon,
and visibility window. It provides a dockable **Top 15** panel with one-click
handoff to the Framing Assistant and then the Advanced Sequencer.

## Install

Tonight's Best requires **N.I.N.A. 3.2.0.9001 or later**.

### From a GitHub release

1. Download `TonightsBest.NINA.Plugin.<version>.zip` from this repository's
   [Releases](https://github.com/CCDASTRO/TonightsBest-NINA/releases) page.
2. Close N.I.N.A.
3. Create this folder if it does not exist:
   `%LOCALAPPDATA%\NINA\Plugins\3.0.0\Tonight's Best`
4. Extract `TonightsBest.Core.dll` and `TonightsBest.NINA.Plugin.dll` from the
   ZIP directly into that folder.
5. Start N.I.N.A. Open **Imaging**, locate **Tonight's Best** among the dockable
   panels, and place or resize it as desired.

N.I.N.A. uses the `3.0.0` compatibility folder for 3.x plugins even though this
plugin requires N.I.N.A. 3.2 or newer.

### Build and install from source

Run `./packaging/build-package.ps1`, then follow the same steps using the ZIP
created in `artifacts`.

## Required setup

Tonight's Best reads N.I.N.A.'s active profile; it does not maintain a duplicate
equipment or location configuration. Before refreshing:

1. Set the observing latitude, longitude, elevation, and optional custom horizon
   in the active N.I.N.A. profile.
2. Select a telescope and enter its effective focal length, including any reducer
   or Barlow effect.
3. Select and connect the camera. N.I.N.A. must report its sensor width, sensor
   height, and pixel size so the plugin can calculate the field of view.
4. Simulator equipment can be used to evaluate the plugin without physical
   hardware.

## Use

1. Open the **Tonight's Best — Top 15** panel in the Imaging workspace.
2. Set the minimum acceptable target altitude; the default is 30°.
3. Click **Refresh Top 15**.
4. Compare object type, overall score, estimated frame coverage, hours above the
   minimum altitude, maximum altitude, Moon separation, and magnitude.
5. Select a row and click **Open in Framing Assistant**.
6. Adjust rotation, framing, or mosaic settings, then use Framing Assistant's
   normal controls to add the target to the Advanced Sequencer.

Frame coverage over 100% means the catalog footprint is larger than the current
camera field. Catalog sizes are estimates and may omit very faint extensions.

## Status

This repository contains a tested **0.1.0 development preview** with the
scoring engine, live N.I.N.A. 3.2 adapters, dockable panel, and Framing Assistant
handoff. It has been compiled, unit tested, queried against an installed N.I.N.A.
Sky Atlas, loaded in N.I.N.A., and exercised end to end with connected simulator
equipment. The verified flow calculated the Top 15, selected Crescent Nebula,
and populated Framing Assistant with its name, coordinates, and active camera
and telescope parameters.

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

The score is deliberately explainable and retained as a component breakdown:

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
patterns used by this implementation. See the
[verification record](docs/VERIFICATION.md) for the automated and in-host checks.

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
- [x] N.I.N.A. host integration test with connected simulator equipment
- [ ] First GitHub release and official plugin-manifest submission

## License

MIT. N.I.N.A. itself and its SDK packages retain their own licenses.
