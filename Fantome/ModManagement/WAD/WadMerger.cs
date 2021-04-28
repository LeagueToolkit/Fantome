using LeagueToolkit.IO.WadFile;
using System;
using System.Linq;

namespace Fantome.ModManagement.WAD
{
    public static class WadMerger
    {
        public static WadBuilder Merge(WadBuilder wadBase, WadBuilder wadToMerge)
        {
            WadBuilder wadBuilder = new WadBuilder();

            //First add new files and then modify changed ones
            foreach (var entryToMerge in wadToMerge.Entries)
            {
                // Add new entry
                if (wadBase.Entries.TryGetValue(entryToMerge.Key, out WadEntryBuilder baseEntry) is false)
                {
                    wadBuilder.WithEntry(entryToMerge.Value);
                }
                // Modify existing entry
                else if (!entryToMerge.Value.Sha256Checksum.SequenceEqual(baseEntry.Sha256Checksum))
                {
                    wadBuilder.WithEntry(entryToMerge.Value);
                }
                // Copy over entry
                else
                {
                    wadBuilder.WithEntry(baseEntry);
                }
            }

            return wadBuilder;
        }
    }
}
