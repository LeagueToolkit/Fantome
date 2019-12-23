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
            foreach(WADEntry entry in wadMerge.Entries)
            {
                if(!wadBase.Entries.Any(x => x.XXHash == entry.XXHash))
                {
                    wadBase.AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, entry.Type);
                }
                else if(!entry.SHA.SequenceEqual(wadBase.Entries.Single(x => x.XXHash == entry.XXHash).SHA))
                {
                    wadBase.Entries.Single(x => x.XXHash == entry.XXHash).EditData(entry.GetContent(false), entry.UncompressedSize);
                }
            }

            return wadBase;
        }
    }
}
