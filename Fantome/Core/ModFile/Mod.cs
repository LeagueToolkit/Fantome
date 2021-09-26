using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core.Exceptions;

namespace Fantome.Core.ModFile
{
    public class Mod
    {
        private Mod()
        {

        }

        public static Mod Mount(string fileLocation)
        {
            return Mount(File.OpenRead(fileLocation));
        }
        public static Mod Mount(Stream stream)
        {
            using BinaryReader br = new(stream);

            string magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if(magic != "FANTOME\0")
            {
                throw new InvalidModMagicException();
            }

            uint version = br.ReadUInt32();
            if(version != 1)
            {
                throw new InvalidModVersionException();
            }

            uint reserved = br.ReadUInt32();
            
            ulong tocOffset = br.ReadUInt64();
            ulong tocEntryCount = br.ReadUInt64();
            ulong metaOffset = br.ReadUInt64();
            ulong metaSize = br.ReadUInt64();

            return new Mod();
        }
    }
}
