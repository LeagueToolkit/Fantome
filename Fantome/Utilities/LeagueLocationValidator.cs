using System.IO;

namespace Fantome.Utilities
{
    public abstract class LeagueLocationValidator
    {
        public static bool Validate(string leagueLocation)
        {
            return File.Exists(string.Format(@"{0}\League of Legends.exe", leagueLocation));
        }
    }
}
