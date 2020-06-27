using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PathIO = System.IO.Path;

namespace Fantome.MVVM.ViewModels.CreateMod
{
    public class CreateModWadItemViewModel : PropertyNotifier
    {
        public CreateModWadItemType Type { get; }
        public string Name => PathIO.GetFileName(this._path);
        public string Path => this._path;

        private string _path;

        public CreateModWadItemViewModel(CreateModWadItemType type, string path)
        {
            this.Type = type;
            this._path = path;
        }
    }

    public enum CreateModWadItemType
    {
        File,
        Folder
    }
}
