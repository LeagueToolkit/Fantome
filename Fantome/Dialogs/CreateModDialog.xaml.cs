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

namespace Fantome.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateModDialog.xaml
    /// </summary>
    public partial class CreateModDialog : UserControl
    {
        public CreateModDialogViewModel ViewModel { get => this.DataContext as CreateModDialogViewModel; }

        public CreateModDialog()
        {
            InitializeComponent();
        }

        private void AddImage(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = false,
                Title = "Select an image for your mod"
            };

            dialog.Filters.Add(new CommonFileDialogFilter("PNG Files", ".png"));

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
                this.ViewModel.WadLocation = dialog.FileName;
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
                this.ViewModel.RawLocation = dialog.FileName;
            }
        }
    }
}
