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
                    wadBase.AddEntry(entry.XXHash, entry.GetContent(true), entry.Type == EntryType.Compressed || entry.Type == EntryType.ZStandardCompressed);
                }
                else if(!entry.SHA.SequenceEqual(wadBase.Entries.Single(x => x.XXHash == entry.XXHash).SHA))
                {
                    wadBase.Entries.Single(x => x.XXHash == entry.XXHash).EditData(entry.GetContent(true));
                }
            }

            return wadBase;
        }
    }
}
