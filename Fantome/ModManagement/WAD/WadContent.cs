namespace Fantome.ModManagement.WAD
{
    public abstract class WadContent
    {
        public WadContentType Type { get; private set; }
        public string Path { get; private set; }

        public WadContent(WadContentType type, string path)
        {
            this.Type = type;
            this.Path = path;
        }
    }

    public enum WadContentType
    {
        RawFolder,
        WadFolder,
        WadFile
    }
}
