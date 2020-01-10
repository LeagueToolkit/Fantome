using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fantome.Utilities
{
    public static class ThemeHelper
    {
        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public static void LoadTheme()
        {
            Log.Information("Loading Theme from Config");

            bool isDarkTheme = Config.Get<bool>("IsDarkTheme");
            Color primaryColor = ConvertPrimaryColor(Config.Get<PrimaryColor>("PrimaryColor"));
            Color secondaryColor = ConvertSecondaryColor(Config.Get<SecondaryColor>("SecondaryColor"));
            Theme theme = Theme.Create(isDarkTheme ? Theme.Dark : Theme.Light, primaryColor, secondaryColor);

            _paletteHelper.SetTheme(theme);
        }

        public static void ChangeTheme(IBaseTheme theme, Color primaryColor, Color secondaryColor)
        {
            Log.Information("Changing Theme");
            _paletteHelper.SetTheme(Theme.Create(theme, primaryColor, secondaryColor));
        }

        public static Color ConvertPrimaryColor(PrimaryColor primaryColor)
        {
            return SwatchHelper.Lookup[(MaterialDesignColor)primaryColor];
        }
        public static Color ConvertSecondaryColor(SecondaryColor secondaryColor)
        {
            return SwatchHelper.Lookup[(MaterialDesignColor)secondaryColor];
        }
    }
}
