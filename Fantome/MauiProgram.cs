using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MudBlazor.Services;
using Fantome.Data;
using Fluxor;
using Fantome.Utilities;

namespace Fantome
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .RegisterBlazorMauiWebView()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddBlazorWebView();
            builder.Services.AddMudServices();
            builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(MauiProgram).Assembly));
            builder.Services.AddSingleton<WeatherForecastService>();

            InitializationRoutine();

            return builder.Build();
        }

        private static void InitializationRoutine()
        {
            Config.Load();
            Logging.Initialize();
        }
    }
}