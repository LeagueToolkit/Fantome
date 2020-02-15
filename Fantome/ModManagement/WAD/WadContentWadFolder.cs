using PathIO = System.IO.Path;

namespace Fantome.ModManagement.WAD
{
    public sealed class WadContentWadFolder : WadContent
    {
        public WadContentWadFolder(string path) : base(WadContentType.WadFolder, path) { }

        public string GetWadName()
        {
            return PathIO.GetFileName(this.Path);
        }
    }
}
