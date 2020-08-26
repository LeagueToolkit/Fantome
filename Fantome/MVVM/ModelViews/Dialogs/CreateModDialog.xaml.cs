using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fantome.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;

using Image = System.Drawing.Image;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using Fantome.Utilities;
using Fantome.ModManagement;
using Fantome.MVVM.ViewModels.CreateMod;

namespace Fantome.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateModDialog.xaml
    /// </summary>
    public partial class CreateModDialog : UserControl
    {
        public CreateModDialogViewModel ViewModel { get => this.DataContext as CreateModDialogViewModel; }
        public ObservableCollection<ValidationError> ValidationErrors { get; private set; } = new ObservableCollection<ValidationError>();

        private LeagueFileIndex _index;

        public CreateModDialog(LeagueFileIndex index, CreateModDialogViewModel model)
        {
            this._index = index;

            InitializeComponent();

            this.CreateButton.DataContext = this;
            this.DataContext = model;
        }

        private void AddImage(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = false,
                Title = "Select an image for your mod"
            };

            dialog.Filters.Add(new CommonFileDialogFilter("PNG Files", "png"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Image image = Image.FromFile(dialog.FileName);
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bitmap = new BitmapImage();

                image.Save(memoryStream, ImageFormat.Png);

                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                this.ViewModel.Image = bitmap;
            }
        }

        private void SelectWADFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (ValidateWADFolder(dialog.FileName))
                {
                    this.ViewModel.WadLocation = dialog.FileName;
                }
                else
                {
                    DialogHelper.ShowModContentValidationDialog("WAD");
                }
            }
        }
        private void SelectRAWFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (ValidateRAWFolder(dialog.FileName))
                {
                    this.ViewModel.RawLocation = dialog.FileName;
                }
                else
                {
                    DialogHelper.ShowModContentValidationDialog("RAW");
                }
            }
        }
        private bool ValidateWADFolder(string wadFolder)
        {
            IEnumerable<string> directories = Directory.EnumerateDirectories(wadFolder);
            IEnumerable<string> files = Directory.EnumerateFiles(wadFolder);

            //WAD needs to have either directories or files
            if (directories.Count() == 0 && files.Count() == 0)
            {
                return false;
            }

            foreach (string directory in directories)
            {
                char separator = Pathing.GetPathSeparator(directory);
                string directoryName = directory.Substring(directory.LastIndexOf(separator) + 1);
                if (string.IsNullOrEmpty(this._index.FindWADPath(directoryName)))
                {
                    return false;
                }
            }
            foreach (string file in files)
            {
                if (!file.EndsWith(".wad.client"))
                {
                    return false;
                }
            }

            return true;
        }
        private bool ValidateRAWFolder(string rawFolder)
        {
            IEnumerable<string> directories = Directory.EnumerateDirectories(rawFolder);
            IEnumerable<string> files = Directory.EnumerateFiles(rawFolder, "*", SearchOption.AllDirectories);

            //RAW needs to have files, but those files also need to be in folders
            if (directories.Count() == 0 || files.Count() == 0)
            {
                return false;
            }

            foreach (string directory in directories)
            {
                if (directory.EndsWith(".wad.client"))
                {
                    return false;
                }
            }
            foreach (string file in files)
            {
                if (file.EndsWith(".wad.client"))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                this.ValidationErrors.Add(e.Error);
            }
            else
            {
                this.ValidationErrors.Remove(e.Error);
            }
        }
    }
}
