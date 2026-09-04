using System.ComponentModel.Composition;
using NINA.Plugin;
using NINA.Plugin.Interfaces;

namespace TonightsBest.NINA.Plugin;

[Export(typeof(IPluginManifest))]
public sealed class TonightsBestPlugin : PluginBase;

