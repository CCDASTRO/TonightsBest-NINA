# N.I.N.A. SDK and plugin-pattern review

Tonight's Best follows the public patterns used by the N.I.N.A. plugin template
and first-party application interfaces:

- target `NINA.Plugin` 3.2.0.9001 and `net8.0-windows`;
- export the manifest and dockable view model through MEF;
- derive the panel view model from `DockableVM` and provide its WPF template
  through an exported `ResourceDictionary`;
- consume camera and telescope state through N.I.N.A. mediators;
- read location, horizon, focal length, and camera fallback settings from the
  active `IProfileService` profile;
- query `NINA.Astrometry.DatabaseInteraction` for N.I.N.A.'s own Sky Atlas;
- use `INighttimeCalculator` and `AstroUtil` for the darkness window and Moon;
- switch to `ApplicationTab.FRAMINGASSISTANT` and call
  `IFramingAssistantVM.SetCoordinates` for handoff.

The core scoring assembly intentionally has no reference to N.I.N.A., WPF, MEF,
or a database. This keeps astronomy and ranking behavior testable without the
host application and confines SDK changes to the adapter assembly.

## Primary references

- [Official plugin template](https://github.com/isbeorn/nina.plugin.template)
- [NINA.Plugin 3.2.0.9001](https://www.nuget.org/packages/NINA.Plugin/3.2.0.9001)
- [Official N.I.N.A. source](https://github.com/isbeorn/nina)
- [Plugin manifest repository](https://github.com/isbeorn/nina.plugin.manifests)

