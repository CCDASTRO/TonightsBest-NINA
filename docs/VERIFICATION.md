# Verification record

## Automated checks

- Release build completes with zero warnings and zero errors.
- Four core tests cover field of view, Moon penalty, ranked-result limits and
  ordering, and oversized-target frame coverage.
- The adapter smoke test queried the installed N.I.N.A. Sky Atlas and mapped
  16,857 targets, all with friendly and native object types; 15,898 included
  usable catalog dimensions.
- The generated manifest validates against the official N.I.N.A. plugin
  manifest schema.

## N.I.N.A. host check

Verified in N.I.N.A. 3.3 nightly build 057 using connected N.I.N.A. simulator
equipment:

1. N.I.N.A. loaded Tonight's Best 0.1.0.0 through its normal plugin loader.
2. The Imaging workspace displayed the dockable **Tonight's Best — Top 15** panel.
3. The plugin read a 500 mm telescope and calculated a 167′ × 125.5′ field for
   the connected simulator camera.
4. **Refresh Top 15** populated 15 ranked targets with type, score, frame
   coverage, hours above minimum altitude, maximum altitude, Moon separation,
   and magnitude.
5. Crescent Nebula was selected and **Open in Framing Assistant** switched tabs
   and populated the name, J2000 coordinates, sensor size, pixel size, and focal
   length.

Real hardware is not required by the plugin; the same N.I.N.A. mediator and
active-profile paths are used for simulator and physical devices.

