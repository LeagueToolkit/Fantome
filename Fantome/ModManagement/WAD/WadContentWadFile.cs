using System;
using System.Collections.Generic;
using System.Text;

namespace Fantome.ModManagement.WAD
{
    public sealed class WadContentWadFile : WadContent
    {
        public WadContentWadFile(string path) : base(WadContentType.WadFile, path)
        {

        }
    }
}
