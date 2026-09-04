using TonightsBest.NINA.Plugin;
using NINA.Astrometry;
using System.Data.SQLite;

var source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NINA", "NINA.sqlite");
if (!File.Exists(source)) throw new FileNotFoundException("The installed N.I.N.A. Sky Atlas database was not found.", source);
var scratch = Path.Combine(Path.GetTempPath(), $"TonightsBest-{Guid.NewGuid():N}.sqlite");
File.Copy(source, scratch);
try {
    var catalog = new NinaSkyAtlasCatalog(() => null!, () => new DatabaseInteraction($"Data Source={scratch};Pooling=False;"));
    var targets = await catalog.SearchAsync(CancellationToken.None);
    if (targets.Count == 0) throw new InvalidOperationException("N.I.N.A. Sky Atlas returned no targets.");
    var typed = targets.Count(target => !string.IsNullOrWhiteSpace(target.ObjectType));
    var nativeTyped = targets.Count(target => !string.IsNullOrWhiteSpace(target.CatalogTypeCode));
    var sized = targets.Count(target => target.MajorAxisArcMinutes > 0);
    if (nativeTyped == 0) throw new InvalidOperationException("Sky Atlas returned no native object-type codes.");
    Console.WriteLine($"Sky Atlas smoke test passed: {targets.Count} targets; {typed} friendly types; {nativeTyped} native types; {sized} with dimensions.");
    foreach (var target in targets.Take(5))
        Console.WriteLine($"{target.Name} | {target.ObjectType} | {target.MajorAxisArcMinutes:0.##}' × {target.MinorAxisArcMinutes:0.##}'");
} finally {
    SQLiteConnection.ClearAllPools();
    try {
        File.Delete(scratch);
    } catch (IOException) {
        Console.WriteLine($"Temporary database will be removable after this process exits: {scratch}");
    }
}
