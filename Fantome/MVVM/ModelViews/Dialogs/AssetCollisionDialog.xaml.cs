using Fantome.ModManagement.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fantome.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for AssetCollisionDialog.xaml
    /// </summary>
    public partial class AssetCollisionDialog : UserControl
    {
        public string Message { get; }
        public List<string> Collisions { get; }

        public AssetCollisionDialog(ModFile mod, List<string> collisions)
        {
            this.DataContext = this;
            this.Message = "Fantome has detected that the following mods have asset collisions with: " + mod.GetID() + '\n';
            this.Message += "Do you want to uninstall these mods to install: " + mod.GetID() + '?';
            this.Collisions = collisions;

            InitializeComponent();
        }
    }
}
