using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core.Exceptions;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.IO.WadFile;
using Newtonsoft.Json;
using Serilog;
using Windows.Storage;

namespace Fantome.Core
{
    public static class GameIndex
    {
        private const string FILE_PATH = "GAME_INDEX.json";

        public static GameIndexStorage ScanGameLocation(string gameLocation)
        {
            Log.Information("Scanning Game location: " + gameLocation);

            GameIndexStorage index = new();
            foreach(string wadFilePath in Directory.EnumerateFiles(gameLocation, "*.wad.client", SearchOption.AllDirectories))
            {
                try
                {
                    string relativeWadFilePath = wadFilePath.Replace(gameLocation + Path.DirectorySeparatorChar, "");
                    using Wad wad = Wad.Mount(wadFilePath, false);

                    index.WadToEntriesMap.TryAdd(relativeWadFilePath, wad.Entries.Keys.ToList());
                }
                catch (InvalidFileSignatureException exception)
                {
                    throw new CorruptedGameFolderException(wadFilePath, exception);
                }
                catch (Exception exception)
                {
                    throw new GameIndexScanException(exception);
                }
            }

            return index;
        }

        public static async Task<GameIndexStorage> FetchAsync()
        {
            StorageFile indexFile = await ApplicationData.Current.LocalFolder.GetFileAsync(FILE_PATH);
            string indexFileContent = await FileIO.ReadTextAsync(indexFile);

            return JsonConvert.DeserializeObject<GameIndexStorage>(indexFileContent);
        }
    }

    public sealed class GameIndexStorage
    {
        public Dictionary<string, List<ulong>> WadToEntriesMap { get; set; } = new();
    }
}
