using Fantome.Services;
using Fantome.Store.Middlewares;
using Fantome.Utilities;
using Fluxor;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MudBlazor;
using MudBlazor.Services;

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
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 200;
                config.SnackbarConfiguration.ShowTransitionDuration = 200;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;

            });

#if DEBUG
            builder.Services.AddFluxor(options => options
                .ScanAssemblies(typeof(MauiProgram).Assembly)
                .UseReduxDevTools()
                .AddMiddleware<LoggerMiddleware>());
#elif !DEBUG
            builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(MauiProgram).Assembly).AddMiddleware<LoggerMiddleware>()););
#endif

            builder.Services.AddSingleton<IWadRepositoryService, WadRepositoryService>();

            InitializationRoutine();

            return builder.Build();
        }

        private static void InitializationRoutine()
        {
            Config.Load();
        }
    }
}