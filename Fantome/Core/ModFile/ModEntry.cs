using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Core.ModFile
{
    public class ModEntry
    {
        public string Name { get; private set; }
        public string Comment { get; private set; }

        public ulong DataOffset { get; private set; }
        public ulong DataLengthCompressed { get; private set; }
        public ulong DataLengthUncompressed { get; private set; }
        public ulong CompressedDataXXHash3 { get; private set; }
        public ulong UncompressedDataXXHash3 { get; private set; }
        public ModEntryCompressionType CompressionType { get; private set; }

        public static ModEntry Read(BinaryReader br)
        {
            ulong entrySize = br.ReadUInt64();
            ulong dataOffset = br.ReadUInt64();
            ulong dataLengthCompressed = br.ReadUInt64();
            ulong dataLengthUncompressed = br.ReadUInt64();
            ulong compressedDataXXHash3 = br.ReadUInt64();
            ulong uncompressedDataXXHash3 = br.ReadUInt64();

            ulong reserved1 = br.ReadUInt64();
            ulong reserved2 = br.ReadUInt64();

            ModEntryCompressionType compressionType = (ModEntryCompressionType)br.ReadUInt32();

            string name = Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32())).Trim('\0');
            string comment = Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32())).Trim('\0');

            return new ModEntry() 
            {
                Name = name,
                Comment = comment, 
                DataOffset = dataOffset,
                DataLengthCompressed = dataLengthCompressed,
                DataLengthUncompressed = dataLengthUncompressed,
                CompressedDataXXHash3 = compressedDataXXHash3,
                UncompressedDataXXHash3 = uncompressedDataXXHash3,
            };
        }
    }

    public enum ModEntryCompressionType : uint
    {
        None = 0,
        ZSTD = 1
    }
}
