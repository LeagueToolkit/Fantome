using Fantome.ModManagement.IO;
using Fantome.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for GeneratingWadFilesDialog.xaml
    /// </summary>
    public partial class GeneratingWadFilesDialog : UserControl
    {
        public string GeneratingString { get; }

        private ModFile _mod;

        public GeneratingWadFilesDialog(ModFile mod)
        {
            this.DataContext = this;
            this._mod = mod;

            this.GeneratingString = string.Format("Generating WAD files for {0}...", mod.GetID());

            InitializeComponent();
        }

        public void StartGeneration(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += GenerateWadFiles;
            worker.RunWorkerCompleted += CloseDialog;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync();
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private void GenerateWadFiles(object sender, DoWorkEventArgs e)
        {
            this._mod.GenerateWadFiles();
        }
    }
}
