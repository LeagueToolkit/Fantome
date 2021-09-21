using System;

namespace Fantome.Core.Exceptions
{
    public class CorruptedGameFolderException : Exception
    {
        public string WadFilePath { get; private set; }

        public CorruptedGameFolderException() : base("Corrupted Game folder")
        {

        }
        public CorruptedGameFolderException(string wadFilePath, Exception innerException) : base("Corrupted Game folder - broken WAD file", innerException)
        {
            this.WadFilePath = wadFilePath;
        }
    }
}
