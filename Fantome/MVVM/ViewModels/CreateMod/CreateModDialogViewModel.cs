using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.Commands;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SImage = System.Drawing.Image;

namespace Fantome.MVVM.ViewModels.CreateMod
{
    public class CreateModDialogViewModel : PropertyNotifier
    {
        public string WadLocation
        {
            get => this._wadLocation;
            set
            {
                this._wadLocation = value;
                NotifyPropertyChanged();
            }
        }
        public string RawLocation
        {
            get => this._rawLocation;
            set
            {
                this._rawLocation = value;
                NotifyPropertyChanged();
            }
        }
        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                NotifyPropertyChanged();
            }
        }
        public string Author
        {
            get => this._author;
            set
            {
                this._author = value;
                NotifyPropertyChanged();
            }
        }
        public string Version
        {
            get => this._version;
            set
            {
                this._version = value;
                NotifyPropertyChanged();
            }
        }
        public BitmapImage Image
        {
            get => this._image;
            set
            {
                this._image = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<CreateModWadItemViewModel> WadItems { get; set; } = new ObservableCollection<CreateModWadItemViewModel>();

        private string _wadLocation;
        private string _rawLocation;
        private string _name;
        private string _author;
        private string _version = "1.0";
        private BitmapImage _image;
        private ModFile _createdMod;
        private LeagueFileIndex _index;

        public ICommand AddWadFilesCommand => new RelayCommand(AddWadFiles);
        public ICommand AddWadFoldersCommand => new RelayCommand(AddWadFolders);
        public ICommand RemoveWadFileCommand => new RelayCommand(RemoveWadItem);

        public CreateModDialogViewModel(LeagueFileIndex index)
        {
            this._index = index;
        }

        public async void AddWadFiles(object parameter)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = true
            };
            List<string> alreadyAddedWadFiles = new List<string>();
            List<string> invalidWadFiles = new List<string>();

            dialog.Filters.Add(new CommonFileDialogFilter("wad.client Files", "wad.client"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //Adding and validation of WAD files
                foreach(string wadFilePath in dialog.FileNames)
                {
                    string wadName = Path.GetFileName(wadFilePath);

                    if (!ValidateWadItemPath(wadName))
                    {
                        invalidWadFiles.Add(wadName);
                    }
                    else if (this.WadItems.Any(x => x.Name == wadName))
                    {
                        alreadyAddedWadFiles.Add(wadName);
                    }
                    else
                    {
                        this.WadItems.Add(new CreateModWadItemViewModel(CreateModWadItemType.File, wadFilePath));
                    }
                }

                if(alreadyAddedWadFiles.Count != 0)
                {
                    await ShowAlreadyAddedWadItems(alreadyAddedWadFiles);
                }
                if(invalidWadFiles.Count != 0)
                {
                    await ShowInvalidWadItems(invalidWadFiles);
                }
            }
        }
        public async void AddWadFolders(object parameter)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Multiselect = true,
                IsFolderPicker = true
            };
            List<string> alreadyAddedWadFiles = new List<string>();
            List<string> invalidWadFolders = new List<string>();

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach (string wadFolderPath in dialog.FileNames)
                {
                    string wadName = Path.GetFileName(wadFolderPath);

                    if (!ValidateWadItemPath(wadName))
                    {
                        invalidWadFolders.Add(wadName);
                    }
                    else if (this.WadItems.Any(x => x.Name == wadName))
                    {
                        alreadyAddedWadFiles.Add(wadName);
                    }
                    else
                    {
                        this.WadItems.Add(new CreateModWadItemViewModel(CreateModWadItemType.Folder, wadFolderPath));
                    }
                }

                if (alreadyAddedWadFiles.Count != 0)
                {
                    await ShowAlreadyAddedWadItems(alreadyAddedWadFiles);
                }
                if (invalidWadFolders.Count != 0)
                {
                    await ShowInvalidWadItems(invalidWadFolders);
                }
            }
        }

        public bool ValidateWadItemPath(string path)
        {
            return !string.IsNullOrEmpty(this._index.FindWADPath(Path.GetFileName(path)));
        }

        public void RemoveWadItem(object parameter)
        {
            string wadName = parameter as string;
            if (!string.IsNullOrEmpty(wadName))
            {
                this.WadItems.Remove(this.WadItems.FirstOrDefault(x => x.Name == wadName));
            }
        }

        public async Task ShowInvalidWadItems(IEnumerable<string> invalidWadNames)
        {
            string message = "The following WAD files are invalid:\n";
            foreach (string invalidWadName in invalidWadNames)
            {
                message += invalidWadName + '\n';
            }

            await DialogHelper.ShowMessageDialog(message);
        }
        public async Task ShowAlreadyAddedWadItems(IEnumerable<string> alreadyAddedWadFiles)
        {
            string message = "The following WAD files were already added:\n";
            foreach (string alreadyAddedWadFile in alreadyAddedWadFiles)
            {
                message += alreadyAddedWadFile + '\n';
            }

            await DialogHelper.ShowMessageDialog(message);
        }

        public void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == true)
            {
                IEnumerable<string> wadFiles = this.WadItems
                    .Where(x => x.Type == CreateModWadItemType.File)
                    .Select(x => x.Path);

                IEnumerable<string> wadFolders = this.WadItems
                    .Where(x => x.Type == CreateModWadItemType.Folder)
                    .Select(x => x.Path);

                ModInfo info = new ModInfo(this._name, this._author, this._version, "");
                SImage image = this._image == null ? null : SImage.FromStream(this._image.StreamSource);
                ModFile mod = new ModFile(this._index, wadFiles, wadFolders, info, image);

                this._createdMod = mod;
            }
        }

        public ModFile GetCreatedMod()
        {
            return this._createdMod;
        }
    }
}
