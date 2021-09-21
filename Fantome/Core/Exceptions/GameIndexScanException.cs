using System;

namespace Fantome.Core.Exceptions
{
    public class GameIndexScanException : Exception 
    {
        public GameIndexScanException(Exception innerException) : base("Failed to scan Game folder", innerException) { }
    }
}
