using System.Linq;

namespace Fantome.Utilities
{
    public static class Pathing
    {
        public static char GetPathSeparator(string path)
        {
            if (path.Contains('\\'))
            {
                return '\\';
            }
            else
            {
                return '/';
            }
        }
        public static char GetInvertedPathSeparator(char separator)
        {
            if (separator == '\\')
            {
                return '/';
            }
            else
            {
                return '\\';
            }
        }
    }
}
