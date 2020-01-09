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
using Fantome.Libraries.League.IO.WAD;
using Fantome.Libraries.League.Helpers.Cryptography;

namespace Fantome.ModManagement.IO
{
    public class ModFile : IEquatable<ModFile>, IDisposable
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
        public Dictionary<string, WADFile> WadFiles
        {
            get
            {
                if (this._wadFiles == null)
                {
                    this._wadFiles = GetWadFiles();
                }

                return this._wadFiles;
            }
        }

        private ModInfo _info;
        private Image _image;
        private Dictionary<string, WADFile> _wadFiles;
        private ModManager _modManager;

        public ModFile(ModManager modManager, string fileLocation)
        {
            this._modManager = modManager;
            this.Content = new ZipArchive(File.OpenRead(fileLocation));
        }
        public ModFile(ModManager modManager, string wadLocation, string rawLocation, ModInfo info, Image image)
        {
            this._modManager = modManager;
            using (FileStream fileStream = new FileStream(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, info.CreateID()), FileMode.Create))
            {
                using (this.Content = new ZipArchive(fileStream, ZipArchiveMode.Update))
                {
                    this._info = info;
                    this._image = image;

                    if (!string.IsNullOrEmpty(wadLocation))
                    {
                        //Check if we want to pack the WAD folders into WAD files
                        if (Config.Get<bool>("PackWadFolders"))
                        {
                            PackWadFolders(wadLocation);
                        }
                        else
                        {
                            AddFolder("WAD", wadLocation);
                        }
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
        private void PackWadFolders(string wadLocation)
        {
            //Loop through each WAD folder
            foreach (string wadFolder in Directory.EnumerateDirectories(wadLocation))
            {
                char separator = Pathing.GetPathSeparator(wadFolder);
                string wadName = wadFolder.Split(separator).Last();

                using (WADFile wad = new WADFile(3, 0))
                {
                    //Add each file to the WAD
                    foreach (string wadFolderFile in Directory.EnumerateFiles(wadFolder, "*", SearchOption.AllDirectories))
                    {
                        string path = wadFolderFile.Replace(wadFolder + separator, "").Replace('\\', '/');
                        ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));
                        string extension = Path.GetExtension(wadFolderFile);

                        wad.AddEntry(hash, File.ReadAllBytes(wadFolderFile), extension != ".wpk" && extension != ".bnk" ? true : false);
                    }

                    //After WAD creation is finished we can write the WAD to the ZIP
                    ZipArchiveEntry archiveEntry = this.Content.CreateEntry(string.Format(@"WAD\{0}", wadName));
                    wad.Write(archiveEntry.Open());
                }
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

        public string Validate(ModManager modManager)
        {
            bool invalidWADFolder = false;

            //Get all files in the WAD folder
            string wadFolderError = string.Format("The WAD folder of {0} contains invalid entries:\n", GetID());
            foreach (ZipArchiveEntry entry in GetEntries(@"WAD[\\/].*"))
            {
                if (!entry.FullName.Contains(".wad.client"))
                {
                    invalidWADFolder = true;
                    wadFolderError += entry.FullName + '\n';
                }
                else
                {
                    //See if the WAD file exists in the game
                    string wadName = entry.FullName.Split(Pathing.GetPathSeparator(entry.FullName))[1];
                    if (string.IsNullOrEmpty(modManager.Index.FindWADPath(wadName)))
                    {
                        invalidWADFolder = true;
                        wadFolderError += entry.FullName + '\n';
                    }
                }
            }
            if (invalidWADFolder)
            {
                return wadFolderError;
            }


            //Get all files in RAW folder and see if they contain a reference to WAD files
            bool invalidRawFolder = false;
            string rawFolderError = string.Format("The RAW folder of {0} contains invalid entries:\n", GetID());
            foreach (ZipArchiveEntry entry in GetEntries(@"RAW[\\/].*(?![\\/])"))
            {
                if (entry.FullName.Contains(".wad.client"))
                {
                    invalidRawFolder = true;
                    rawFolderError += entry.FullName + '\n';
                }
            }
            if (invalidRawFolder)
            {
                return rawFolderError;
            }

            return "";
        }
        public void GenerateWadFiles()
        {
            this._wadFiles = GetWadFiles();
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
        private Dictionary<string, WADFile> GetWadFiles()
        {
            Dictionary<string, WADFile> modWadFiles = new Dictionary<string, WADFile>();

            //Collect WAD files in WAD folder
            CollectWADFiles();

            //Pack WAD folders files into WAD files
            CollectWADFolders();

            //Collect files from the RAW folder
            CollectRAWFiles();

            return modWadFiles;

            void CollectWADFiles()
            {
                foreach (ZipArchiveEntry zipEntry in GetEntries(@"WAD[\\/][\w.]+.wad.client(?![\\/])"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string wadPath = this._modManager.Index.FindWADPath(zipEntry.FullName.Split(ps)[1]);

                    zipEntry.ExtractToFile("wadtemp", true);
                    modWadFiles.Add(wadPath, new WADFile(new MemoryStream(File.ReadAllBytes("wadtemp"))));
                    File.Delete("wadtemp");

                    //We need to check each entry to see if they're shared across any other WAD files
                    //if they are, we need to also modify those WADs
                    foreach (WADEntry entry in modWadFiles[wadPath].Entries)
                    {
                        //Check if the entry is present in the game files or if it's new
                        if (this._modManager.Index.Game.ContainsKey(entry.XXHash))
                        {
                            foreach (string additionalWadPath in this._modManager.Index.Game[entry.XXHash].Where(x => x != wadPath))
                            {
                                if (!modWadFiles.ContainsKey(additionalWadPath))
                                {
                                    modWadFiles.Add(additionalWadPath, new WADFile(3, 0));
                                }

                                if (entry.Type == EntryType.Uncompressed)
                                {
                                    modWadFiles[additionalWadPath].AddEntry(entry.XXHash, entry.GetContent(false), false);
                                }
                                else if (entry.Type == EntryType.Compressed || entry.Type == EntryType.ZStandardCompressed)
                                {
                                    modWadFiles[additionalWadPath].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, entry.Type);
                                }
                            }
                        }
                    }
                }
            }
            void CollectWADFolders()
            {
                List<string> wadPaths = new List<string>();

                foreach (ZipArchiveEntry zipEntry in GetEntries(@"WAD[\\/][\w.]+.wad.client[\\/].*"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string wadName = zipEntry.FullName.Split(ps)[1];
                    string wadPath = this._modManager.Index.FindWADPath(wadName);
                    string path = zipEntry.FullName.Replace(string.Format("WAD{0}{1}{0}", ps, wadName), "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                    if (!modWadFiles.ContainsKey(wadPath))
                    {
                        modWadFiles.Add(wadPath, new WADFile(3, 0));
                        wadPaths.Add(wadPath);
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        zipEntry.Open().CopyTo(memoryStream);
                        if (Path.GetExtension(path) == ".wpk")
                        {
                            modWadFiles[wadPath].AddEntry(hash, memoryStream.ToArray(), false);
                        }
                        else
                        {
                            modWadFiles[wadPath].AddEntry(hash, memoryStream.ToArray(), true);
                        }
                    }
                }

                //Shared Entry Check
                foreach (string wadPath in wadPaths)
                {
                    foreach (WADEntry entry in modWadFiles[wadPath].Entries)
                    {
                        //Check if the entry is present in the game files or if it's new
                        if (this._modManager.Index.Game.ContainsKey(entry.XXHash))
                        {
                            foreach (string additionalWadPath in this._modManager.Index.Game[entry.XXHash].Where(x => x != wadPath))
                            {
                                if (!modWadFiles.ContainsKey(additionalWadPath))
                                {
                                    modWadFiles.Add(additionalWadPath, new WADFile(3, 0));
                                }

                                modWadFiles[additionalWadPath].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, EntryType.ZStandardCompressed);
                            }
                        }
                    }
                }
            }
            void CollectRAWFiles()
            {
                foreach (ZipArchiveEntry zipEntry in GetEntries(@"RAW[\\/].*"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string path = zipEntry.FullName.Replace(@"RAW" + ps, "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));
                    List<string> fileWadFiles = new List<string>();

                    //Check if file exists, if not, we discard it
                    if (this._modManager.Index.Game.ContainsKey(hash))
                    {
                        fileWadFiles = this._modManager.Index.Game[hash];
                        foreach (string wadFilePath in fileWadFiles)
                        {
                            if (!modWadFiles.ContainsKey(wadFilePath))
                            {
                                modWadFiles.Add(wadFilePath, new WADFile(3, 0));
                            }

                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                zipEntry.Open().CopyTo(memoryStream);
                                modWadFiles[wadFilePath].AddEntry(hash, memoryStream.ToArray(), true);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Content.Dispose();
            DisposeWADFiles();

            //This is very bad but it works :( if someone finds where there is a memory leak happening please let me now
            GC.Collect();
        }
        public void DisposeWADFiles()
        {
            if (this._wadFiles != null)
            {
                foreach (KeyValuePair<string, WADFile> wad in this._wadFiles)
                {
                    wad.Value.Dispose();
                }
            }

            this._wadFiles = null;
        }
        public void DisposeReopen()
        {
            Dispose();

            this.Content = ZipFile.OpenRead(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, GetID()));
        }

        public bool Equals(ModFile other)
        {
            return this == other;
        }
        public static bool operator ==(ModFile mod1, ModFile mod2)
        {
            return mod1.Info == mod2.Info;
        }
        public static bool operator !=(ModFile mod1, ModFile mod2)
        {
            return mod1.Info != mod2.Info;
        }
    }
}
