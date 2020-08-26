using Fantome.Libraries.League.IO.WAD;
using System;
using System.Linq;

namespace Fantome.ModManagement.WAD
{
    public static class WadMerger
    {
        public static WADFile Merge(WADFile wadBase, WADFile wadMerge)
        {
            //First add new files and then modify changed ones
            foreach (WADEntry entry in wadMerge.Entries)
            {
                WADEntry baseEntry = wadBase.Entries.FirstOrDefault(x => x.XXHash == entry.XXHash);

                if (baseEntry == null)
                {
                    if (entry.Type == EntryType.Uncompressed)
                    {
                        wadBase.AddEntry(entry.XXHash, entry.GetContent(false), false);
                    }
                    else if (entry.Type == EntryType.ZStandardCompressed || entry.Type == EntryType.Compressed)
                    {
                        wadBase.AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, entry.Type);
                    }
                }
                else if (!entry.SHA.SequenceEqual(baseEntry.SHA))
                {
                    if (entry.Type == EntryType.Uncompressed)
                    {
                        wadBase.Entries.Single(x => x.XXHash == entry.XXHash).EditData(entry.GetContent(false));
                    }
                    else if (entry.Type == EntryType.ZStandardCompressed || entry.Type == EntryType.Compressed)
                    {
                        wadBase.Entries.Single(x => x.XXHash == entry.XXHash).EditData(entry.GetContent(false), entry.UncompressedSize);
                    }
                }
            }

            return wadBase;
        }

        public static WADFile Merge(WADFile wadBase, WADFile wadMerge, out bool returnedMerge)
        {
            returnedMerge = false;

            //If the Modded WAD mods all entries of the original WAD then it will be returned to prevent memory halting and increase speed
            bool containsBaseEntries = true;
            foreach (WADEntry baseEntry in wadBase.Entries)
            {
                if (!wadMerge.Entries.Any(x => x.XXHash == baseEntry.XXHash))
                {
                    containsBaseEntries = false;
                    break;
                }
            }

            if (containsBaseEntries)
            {
                returnedMerge = containsBaseEntries;
                return wadMerge;
            }
            else
            {
                return Merge(wadBase, wadMerge);
            }
        }
    }
}
