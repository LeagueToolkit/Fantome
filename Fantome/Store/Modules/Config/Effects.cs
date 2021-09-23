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

namespace Fantome.Store.Modules.Config
{
    public class Effects
    {
        private const string CONFIG_PATH = "CONFIG.json";

        private readonly IState<ConfigState> _configState;

        public Effects(IState<ConfigState> configState)
        {
            this._configState = configState;
        }

        [EffectMethod]
        public async Task HandleConfigAction(ConfigAction action, IDispatcher dispatcher) => await SaveConfig();

        [EffectMethod]
        public async Task HandleFetchConfigRequestAction(FetchConfigAction.Request action, IDispatcher dispatcher)
        {
            try
            {
                StorageFile configFile = await ApplicationData.Current.LocalFolder.GetFileAsync(CONFIG_PATH);
                string configSerialized = await FileIO.ReadTextAsync(configFile);

                Log.Information("Loaded Config");

                dispatcher.Dispatch(new SetConfigAction()
                {
                    Config = JsonConvert.DeserializeObject<ConfigState>(configSerialized)
                });
                dispatcher.Dispatch(new FetchConfigAction.Success());
            }
            catch (FileNotFoundException)
            {
                await SaveConfig();
                dispatcher.Dispatch(new FetchConfigAction.Success());
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to load Config");
                dispatcher.Dispatch(new FetchConfigAction.Failure() { Error = exception });
            }
        }

        private async Task SaveConfig()
        {
            StorageFile configFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(CONFIG_PATH, CreationCollisionOption.OpenIfExists);
            string configSerialized = JsonConvert.SerializeObject(this._configState.Value, Formatting.Indented);

            Log.Information("Saving Config...");

            await FileIO.WriteTextAsync(configFile, configSerialized);
        }
    }
}
