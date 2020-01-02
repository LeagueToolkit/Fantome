using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fantome.Utilities;

namespace Fantome.ModManagement.IO
{
    public class ModFile
    {
        public ModInfo Info
        {
            get
            {
                if (this._info == null)
                {
                    this._info = GetPackageInfo();
                }

                return this._info;
            }
        }
        public Image Image
        {
            get
            {
                if (this._image == null)
                {
                    this._image = GetPackageImage();
                }

                return this._image;
            }
        }
        public ZipArchive Content { get; private set; }

        private ModInfo _info;
        private Image _image;

        public ModFile(string fileLocation)
        {
            this.Content = new ZipArchive(File.OpenRead(fileLocation));
        }
        public ModFile(string wadLocation, string rawLocation, ModInfo info, Image image)
        {
            using (FileStream fileStream = new FileStream(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, info.CreateID()), FileMode.Create))
            {
                using (this.Content = new ZipArchive(fileStream, ZipArchiveMode.Update))
                {
                    this._info = info;
                    this._image = image;

                    if (!string.IsNullOrEmpty(wadLocation))
                    {
                        AddFolder("WAD", wadLocation);
                    }

                    if (!string.IsNullOrEmpty(rawLocation))
                    {
                        AddFolder("RAW", rawLocation);
                    }

                    AddFile(@"META\info.json", Encoding.ASCII.GetBytes(info.Serialize()));

                    if (image != null)
                    {
                        using (MemoryStream imageStream = new MemoryStream())
                        {
                            image.Save(imageStream, ImageFormat.Png);
                            AddFile(@"META\image.png", imageStream.ToArray());
                        }
                    }
                }
            }

           this.Content = ZipFile.OpenRead(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, this.GetID()));
        }

        public void AddFolder(string path, string folderLocation)
        {
            foreach (string file in Directory.EnumerateFiles(folderLocation, "*", SearchOption.AllDirectories))
            {
                AddFile(string.Format("{0}\\{1}", path, file.Replace(folderLocation + "\\", "")), File.ReadAllBytes(file));
            }
        }
        public void AddFile(string path, byte[] data)
        {
            ZipArchiveEntry entry = this.Content.CreateEntry(path);

            using (Stream entryStream = entry.Open())
            {
                entryStream.Write(data, 0, data.Length);
            }
        }

        public ZipArchiveEntry GetEntry(string path)
        {
            char separator = Pathing.GetPathSeparator(path);
            ZipArchiveEntry entry = this.Content.GetEntry(path);
            if (entry == null)
            {
                entry = this.Content.GetEntry(path.Replace(separator, Pathing.GetInvertedPathSeparator(separator)));
            }

            return entry;
        }
        public IEnumerable<ZipArchiveEntry> GetEntries(string regexPattern)
        {
            return this.Content.Entries.Where(x => Regex.IsMatch(x.FullName, regexPattern));
        }

        public string GetID()
        {
            return this.Info.CreateID();
        }
        private ModInfo GetPackageInfo()
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                GetEntry(@"META\info.json").Open().CopyTo(memoryStream);

                return JsonConvert.DeserializeObject<ModInfo>(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }
            catch (NullReferenceException)
            {
                return new ModInfo("NULL", "", new Version(0, 0, 0, 0), "");
            }
        }
        private Image GetPackageImage()
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                GetEntry(@"META\image.png").Open().CopyTo(memoryStream);

                return Image.FromStream(memoryStream);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
