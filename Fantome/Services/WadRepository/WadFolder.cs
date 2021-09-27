using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathing = System.IO.Path;

namespace Fantome.Services.WadRepository
{
    public class WadFolder : WadItem
    {
        public ReadOnlyCollection<WadItem> Items => new(this._items);

        private List<WadItem> _items = new();

        public WadFolder(WadItem parent, string path)
        {
            this.Parent = parent;
            this.Name = Pathing.GetFileName(path);
            this.Path = path;
        }

        public void AddFile(string path)
        {
            string[] pathComponents = path.Split('/');

            // If only path component is name then this is the file folder
            if (pathComponents.Length == 1)
            {
                this._items.Add(new WadFile(this, path));
            }
            else
            {
                // If we get here that means the file is nested in one of the folders of this item
                string nestedPath = string.Join('/', pathComponents.TakeLast(pathComponents.Length - 1));
                string folderName = pathComponents[0];

                if (this._items.Find(x => x.Name == folderName) is WadFolder folder)
                {
                    // Folder exists, pass the file to it without this item's name
                    folder.AddFile(nestedPath);
                }
                else
                {
                    // Folder doesn't exist, create it
                    WadFolder newFolder = new(this, Pathing.Combine(this.Path, folderName));

                    newFolder.AddFile(nestedPath);

                    this._items.Add(newFolder);
                }
            }
        }
    }
}
