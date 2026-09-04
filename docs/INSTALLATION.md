# Installation (development build)

Tonight's Best requires N.I.N.A. 3.2.0.9001 or later.

1. Run `packaging/build-package.ps1` from PowerShell.
2. Close N.I.N.A.
3. Create `%LOCALAPPDATA%\NINA\Plugins\3.0.0\Tonight's Best`. N.I.N.A. uses
   this compatibility folder for 3.x plugins even though Tonight's Best requires
   N.I.N.A. 3.2 or later.
4. Extract the two DLL files from `artifacts\TonightsBest.NINA.Plugin.0.1.0.0.zip`
   into that folder.
5. Start N.I.N.A. and enable **Tonight's Best** in the Imaging workspace.

Before refreshing, select/configure a telescope with a valid focal length,
connect the selected camera so N.I.N.A. reports sensor dimensions and pixel
size, and configure latitude/longitude in the active profile.

For public distribution, publish the generated ZIP as a GitHub release asset,
update its URL if necessary, validate `artifacts/manifest.json` against the
official N.I.N.A. manifest repository, and submit it by pull request.
