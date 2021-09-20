using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using Newtonsoft.Json;
using Serilog;
using Windows.Storage;

namespace Fantome.Store.Middlewares
{
    public class LoggerMiddleware : Middleware
    {
        private static readonly string LOGS_FOLDER = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs");

        private IStore _store;

        public override Task InitializeAsync(IStore store)
        {
            this._store = store;

            string logPath = string.Format(@"{0}\FantomeLog - {1}.txt", LOGS_FOLDER, DateTime.Now.ToString("dd.MM.yyyy - HH-mm-ss"));
            string loggingPattern = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} | [{Level}] | {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, outputTemplate: loggingPattern)
                .CreateLogger();

            Log.Information("Initialized Logger");

            return Task.CompletedTask;
        }

        public override void AfterDispatch(object action)
        {
            Log.Information($"Dispatch: |{action.GetType().Name}| {JsonConvert.SerializeObject(action)}");
        }
    }
}
