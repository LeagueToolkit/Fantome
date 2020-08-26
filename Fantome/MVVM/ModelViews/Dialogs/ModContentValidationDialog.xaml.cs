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
    /// Interaction logic for ModContentValidationDialog.xaml
    /// </summary>
    public partial class ModContentValidationDialog : UserControl
    {
        public string Message { get; private set; } = "";

        public ModContentValidationDialog(string folderType)
        {
            if(folderType == "WAD")
            {
                this.Message += "The content of your selected WAD folder isn't valid.\n";
                this.Message += "You cannot add anything other than .WAD.CLIENT files or folders into the WAD folder\n";
                this.Message += "For more info on the Mod File format visit the Fantome wiki";
            }
            else if(folderType == "RAW")
            {
                this.Message += "The content of your selected RAW folder isn't valid.\n";
                this.Message += "You cannot add .WAD.CLIENT files or folders into the RAW folder\n";
                this.Message += "For more info on the Mod File format visit the Fantome wiki";
            }

            InitializeComponent();

            this.DataContext = this;
        }
    }
}
