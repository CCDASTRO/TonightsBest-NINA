using System.ComponentModel.Composition;
using System.Windows;

namespace TonightsBest.NINA.Plugin;

[Export(typeof(ResourceDictionary))]
public partial class TonightsBestDockableTemplates : ResourceDictionary {
    public TonightsBestDockableTemplates() => InitializeComponent();
}

