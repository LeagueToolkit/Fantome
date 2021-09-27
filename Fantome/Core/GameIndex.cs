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
        public static GameIndexStorage ScanGameLocation(string gameLocation)
        {
            Log.Information("Scanning Game location: " + gameLocation);

            GameIndexStorage index = new();
            foreach (string wadFilePath in Directory.EnumerateFiles(gameLocation, "*.wad.client", SearchOption.AllDirectories))
            {
                try
                {
                    string relativeWadFilePath = wadFilePath.Replace(gameLocation + Path.DirectorySeparatorChar, "");
                    using Wad wad = Wad.Mount(wadFilePath, false);

                    GenerateWadToEntriesMap(index, relativeWadFilePath, wad);
                    GenerateEntryToWadsMap(index, relativeWadFilePath, wad);

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

        private static void GenerateWadToEntriesMap(GameIndexStorage index, string relativeWadFilePath, Wad wad)
        {
            index.WadToEntriesMap.TryAdd(relativeWadFilePath, wad.Entries.Keys.ToList());
        }
        private static void GenerateEntryToWadsMap(GameIndexStorage index, string relativeWadFilePath, Wad wad)
        {
            foreach (KeyValuePair<ulong, WadEntry> entry in wad.Entries)
            {
                if (index.EntryToWadsMap.TryAdd(entry.Key, new() { relativeWadFilePath }) is false)
                {
                    index.EntryToWadsMap[entry.Key].Add(relativeWadFilePath);
                }
            }
        }
    }

    public sealed class GameIndexStorage
    {
        public Dictionary<string, List<ulong>> WadToEntriesMap { get; set; } = new();
        public Dictionary<ulong, List<string>> EntryToWadsMap { get; set; } = new();
    }
}
