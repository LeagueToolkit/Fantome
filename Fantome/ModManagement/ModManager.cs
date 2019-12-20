using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.WAD;
using Fantome.ModManagement.IO;

namespace Fantome.ModManagement
{
    public class ModManager
    {
        public const string MOD_FOLDER = "Mods";
        public const string OVERLAY_FOLDER = "Overlay";

        public LeagueFileIndex Index { get; private set; }
        public string LeagueFolder { get; set; }
        public ModDatabase Database { get; set; }

        public ModManager(string leagueFolder)
        {
            if (!IsValidLeagueFolder(leagueFolder))
            {
                //Error
            }

            this.LeagueFolder = leagueFolder;

            BuildLeagueFileIndex();
        }

        public void BuildLeagueFileIndex()
        {
            this.Index = new LeagueFileIndex(this.LeagueFolder);
        }

        public void InstallMod(ModFile mod)
        {
            Dictionary<string, WADFile> wadFiles = new Dictionary<string, WADFile>();
            bool processedWad = false;

            foreach(ZipArchiveEntry zipEntry in mod.Content.Entries.Where(x => Regex.IsMatch(x.FullName, @"WAD\/\w*.wad.client\/(?![\s\S])")))
            {
                string wadName = zipEntry.FullName.Split('/')[1];

                if(zipEntry.CompressedLength != 0)
                {
                    MemoryStream wadStream = new MemoryStream();
                
                    zipEntry.Open().CopyTo(wadStream);
                    wadFiles.Add(wadName, new WADFile(wadStream));

                    processedWad = true;
                }
                else
                {
                    if (!wadFiles.ContainsKey(wadName))
                    {
                        wadFiles.Add(wadName, new WADFile(3, 0));
                    }
                }
            }

            //Process WAD folder folders
            if(!processedWad)
            {
                foreach(KeyValuePair<string, WADFile> wadFile in wadFiles)
                {
                    foreach(ZipArchiveEntry zipEntry in mod.Content.Entries
                        .Where(x => Regex.IsMatch(x.FullName, string.Format(@"WAD\/{0}\/[\s\S]", wadFile.Key))) //get only WAD entries, files can be extensionless, thus next step is required
                        .Where(x => x.CompressedLength != 0)) //get only files
                    {
                        string path = zipEntry.FullName.Replace(string.Format(@"WAD/{0}/", wadFile.Key), "");
                        ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                        MemoryStream memoryStream = new MemoryStream();
                        zipEntry.Open().CopyTo(memoryStream);

                        wadFile.Value.AddEntry(hash, memoryStream.ToArray(), true);
                    }
                }
            }

            //Now we need to install the WAD files
            foreach (KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                string wadPath = this.Index.FindWADPath(wadFile.Key);
                WADFile originalWad = new WADFile(string.Format(@"{0}\Game\{1}", this.LeagueFolder, wadPath));

                Directory.CreateDirectory(string.Format(@"{0}\{1}", OVERLAY_FOLDER, Path.GetDirectoryName(wadPath)));

                using (WADFile merged = WADMerger.Merge(originalWad, wadFile.Value))
                {
                    merged.Write(string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadPath));
                }
            }

            this.Database.AddMod(mod.GetModIdentifier(), true);
        }

        public void UninstallMod(ModFile mod)
        {

        }

        private Version GetLeagueVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(Path.Combine(this.LeagueFolder, "Game/League of Legends.exe")).FileVersion);
        }
        private bool IsValidLeagueFolder(string leagueFolder)
        {
            return File.Exists(Path.Combine(leagueFolder, "LeagueClient.exe"));
        }
    }
}
