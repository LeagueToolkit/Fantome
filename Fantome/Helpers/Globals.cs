using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Helpers
{
    public class Globals
    {
        public static string LeaguePath { get; set; }
        public static string GetLeaguePath()
        {
            if(LeaguePath == null)
            {
                string Path = "";
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Riot Games\\League of Legends"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("Path");
                        Path = o.ToString();
                    }
                }
                return Path;
            }
            else
            {
                return LeaguePath;
            }
        }
    }
}
