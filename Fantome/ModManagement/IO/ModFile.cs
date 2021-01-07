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

        private ModInfo _info;
        private Image _image;
        private Dictionary<string, WADFile> _wadFiles;

        public bool IsOpen => !this._isDisposed;
        private bool _isDisposed;
        private string _file;

        public ModFile(string fileLocation)
        {
            this.Content = new ZipArchive(File.OpenRead(fileLocation));
            this._file = fileLocation;
        }
        public ModFile(LeagueFileIndex index, IEnumerable<string> wadFilePaths, IEnumerable<string> wadFolderPaths, ModInfo info, Image image)
        {
            using (FileStream fileStream = new FileStream(string.Format(@"{0}\{1}.fantome", ModManager.MOD_FOLDER, info.CreateID()), FileMode.Create))
            {
                using (this.Content = new ZipArchive(fileStream, ZipArchiveMode.Update))
                {
                    this._info = info;
                    this._image = image;

                    foreach(string wadFolderPath in wadFolderPaths)
                    {
                        //Check if we want to pack the WAD folder into a WAD file
                        if (Config.Get<bool>("PackWadFolders"))
                        {
                            PackWadFolder(wadFolderPath);
                        }
                        else
                        {
                            AddFolder("WAD", wadFolderPath);
                        }
                    }

                    foreach(string wadFilePath in wadFilePaths)
                    {
                        string wadFileName = Path.GetFileName(wadFilePath);
                        string wadPath = index.FindWADPath(wadFileName);

                        AddFile($@"WAD\{wadFileName}", File.ReadAllBytes(wadFilePath));
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

            this.Content = ZipFile.OpenRead(string.Format(@"{0}\{1}.fantome", ModManager.MOD_FOLDER, this.GetID()));
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
        private void PackWadFolder(string wadFolderLocation)
        {
            string[] wadFolderFiles = Directory.GetFiles(wadFolderLocation, "*", SearchOption.AllDirectories);
            if (wadFolderFiles.Length > 0)
            {
                char separator = Pathing.GetPathSeparator(wadFolderLocation);
                string wadName = wadFolderLocation.Split(separator).Last();

                using (WADFile wad = new WADFile(3, 0))
                {
                    //Add each file to the WAD
                    foreach (string wadFolderFile in Directory.EnumerateFiles(wadFolderLocation, "*", SearchOption.AllDirectories))
                    {
                        string path = wadFolderFile.Replace(wadFolderLocation + separator, "").Replace('\\', '/');
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

        public string Validate(LeagueFileIndex index)
        {
            string validationError = "";

            //Check for RAW, WAD and META folders, and collect entries from RAW and WAD
            ZipArchiveEntry[] wadEntries = GetEntries(@"^WAD[\\/].+").ToArray();
            ZipArchiveEntry[] rawEntries = GetEntries(@"^RAW[\\/].+").ToArray();
            ZipArchiveEntry[] metaEntries = GetEntries(@"^META[\\/].*").ToArray();

            validationError = ValidateBaseFolders();
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            validationError = ValidateBaseFoldersContent();
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            return string.Empty;

            string ValidateBaseFolders()
            {
                if (wadEntries.Length == 0 && rawEntries.Length == 0)
                {
                    return string.Format("{0} contains no files in either WAD or RAW folder", GetID());
                }

                //Check if META folder exists
                if (metaEntries.Length != 0)
                {
                    //If it does then we check if it contains info.json
                    if (!metaEntries.Any(x => x.Name == "info.json"))
                    {
                        return string.Format("The META folder of {0} does not contain a META/info.json file", GetID());
                    }
                }
                else
                {
                    return string.Format("{0} does not contain a META folder", GetID());
                }

                return string.Empty;
            }
            string ValidateBaseFoldersContent()
            {
                bool isInvalid = false;

                //Get all files in the WAD folder
                validationError = string.Format("The WAD folder of {0} contains invalid entries:\n", GetID());
                foreach (ZipArchiveEntry entry in wadEntries)
                {
                    if (!entry.FullName.Contains(".wad.client"))
                    {
                        isInvalid = true;
                        validationError += entry.FullName + '\n';
                    }
                    else
                    {
                        //See if the WAD file exists in the game
                        string wadName = entry.FullName.Split(Pathing.GetPathSeparator(entry.FullName))[1];
                        if (string.IsNullOrEmpty(index.FindWADPath(wadName)))
                        {
                            isInvalid = true;
                            validationError += entry.FullName + '\n';
                        }
                    }
                }
                if (isInvalid)
                {
                    return validationError;
                }


                //Get all files in RAW folder and see if they contain a reference to WAD files
                validationError = string.Format("The RAW folder of {0} contains invalid entries:\n", GetID());
                foreach (ZipArchiveEntry entry in rawEntries)
                {
                    if (entry.FullName.Contains(".wad.client"))
                    {
                        isInvalid = true;
                        validationError += entry.FullName + '\n';
                    }
                }
                if (isInvalid)
                {
                    return validationError;
                }

                return string.Empty;
            }
        }
        public void GenerateWadFiles(LeagueFileIndex index)
        {
            this._wadFiles = GetWadFiles(index);
        }

        public string GetID()
        {
            return this.Info.CreateID();
        }
        private ModInfo GetPackageInfo()
        {
            ZipArchiveEntry entry = GetEntry(@"META\info.json");
            ModInfo currentModInfo = null;
            if (entry != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                entry.Open().CopyTo(memoryStream);

                currentModInfo = JsonConvert.DeserializeObject<ModInfo>(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }

            return currentModInfo ?? new ModInfo(Path.GetFileNameWithoutExtension(this._file),  "unknown", "0.0", "");
        }
        private Image GetPackageImage()
        {
            MemoryStream memoryStream = new MemoryStream();
            ZipArchiveEntry entry = GetEntry(@"META\image.png");
            if (entry != null)
            {
                entry.Open().CopyTo(memoryStream);

                return Image.FromStream(memoryStream);
            }

            return null;
        }

        public Dictionary<string, WADFile> GetWadFiles(LeagueFileIndex index)
        {
            if (this._wadFiles != null)
            {
                return this._wadFiles;
            }

            Dictionary<string, WADFile> modWadFiles = new Dictionary<string, WADFile>();

            //Collect WAD files in WAD folder
            CollectWADFiles();

            //Pack WAD folders files into WAD files
            CollectWADFolders();

            //Collect files from the RAW folder
            CollectRAWFiles();

            this._wadFiles = modWadFiles;

            return modWadFiles;

            void CollectWADFiles()
            {
                foreach (ZipArchiveEntry zipEntry in GetEntries(@"^WAD[\\/][\w.]+.wad.client$"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string wadPath = index.FindWADPath(zipEntry.FullName.Split(ps)[1]);

                    zipEntry.ExtractToFile("wadtemp", true);
                    modWadFiles.Add(wadPath, new WADFile(new MemoryStream(File.ReadAllBytes("wadtemp"))));
                    File.Delete("wadtemp");

                    //We need to check each entry to see if they're shared across any other WAD files
                    //if they are, we need to also modify those WADs
                    foreach (WADEntry entry in modWadFiles[wadPath].Entries)
                    {
                        //Check if the entry is present in the game files or if it's new
                        if (index.Game.ContainsKey(entry.XXHash))
                        {
                            foreach (string additionalWadPath in index.Game[entry.XXHash].Where(x => x != wadPath))
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

                foreach (ZipArchiveEntry zipEntry in GetEntries(@"^WAD[\\/][\w.]+.wad.client[\\/].*"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string wadName = zipEntry.FullName.Split(ps)[1];
                    string wadPath = index.FindWADPath(wadName);
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
                        if (index.Game.ContainsKey(entry.XXHash))
                        {
                            foreach (string additionalWadPath in index.Game[entry.XXHash].Where(x => x != wadPath))
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
                foreach (ZipArchiveEntry zipEntry in GetEntries(@"^RAW[\\/].*"))
                {
                    char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                    string path = zipEntry.FullName.Replace(@"RAW" + ps, "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                    //Check if file exists, if not, we discard it
                    if (index.Game.ContainsKey(hash))
                    {
                        List<string> fileWadFiles = index.Game[hash];
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

        public void Reopen()
        {
            this.Content = new ZipArchive(File.OpenRead(this._file));

            this._isDisposed = false;
        }

        public void Dispose()
        {
            if(!this._isDisposed)
            {
                this.Content.Dispose();
                DisposeWADFiles();

                //This is very bad but it works :( if someone finds where there is a memory leak happening please let me now
                GC.Collect();

                this._isDisposed = true;
            }
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
            Reopen();
        }

        public bool Equals(ModFile other)
        {
            return this == other;
        }
        public static bool operator ==(ModFile mod1, ModFile mod2)
        {
            return mod1?.Info == mod2?.Info;
        }
        public static bool operator !=(ModFile mod1, ModFile mod2)
        {
            return mod1?.Info != mod2?.Info;
        }
    }
}
