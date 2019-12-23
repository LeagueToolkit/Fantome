using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.IO.Compression;
using System.Windows.Media.Imaging;

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
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (this.Content = new ZipArchive(memoryStream, ZipArchiveMode.Update))
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

                File.WriteAllBytes(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, this.GetModIdentifier()), memoryStream.ToArray());
            }

            this.Content = ZipFile.OpenRead(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, this.GetModIdentifier()));
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

        public string GetModIdentifier()
        {
            return string.Format("{0} - {1} (by {2})", this.Info.Name, this.Info.Version.ToString(), this.Info.Author);
        }

        private ModInfo GetPackageInfo()
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                this.Content.GetEntry(@"META\info.json").Open().CopyTo(memoryStream);

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
                this.Content.GetEntry(@"META\image.png").Open().CopyTo(memoryStream);

                return Image.FromStream(memoryStream);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
