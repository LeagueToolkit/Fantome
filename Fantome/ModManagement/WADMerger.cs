using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Libraries.League.IO.WAD;

namespace Fantome.ModManagement
{
    public static class WADMerger
    {
        public static WADFile Merge(WADFile wadBase, WADFile wadMerge)
        {
            //First add new files and then modify changed ones
            foreach (WADEntry entry in wadMerge.Entries)
            {
                WADEntry baseEntry = wadBase.Entries.First(x => x.XXHash == entry.XXHash);

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
    }
}
