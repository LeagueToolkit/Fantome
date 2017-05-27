using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fantome.IO.Config
{
    public class ConfigFile
    {
        public List<Setting> Settings { get; private set; } = new List<Setting>();
        public ConfigFile() { }
        public ConfigFile(string Location)
        {
            using (StreamReader sr = new StreamReader(Location))
            {
                this.Settings = JsonConvert.DeserializeObject<List<Setting>>(sr.ReadToEnd());
            }
        }
        public void Save(string Location)
        {
            using (StreamWriter sw = new StreamWriter(Location))
            {
                sw.Write(JsonConvert.SerializeObject(this.Settings, Formatting.Indented));
            }
        }
        public void AddSetting(string Key, string Value)
        {
            AddSetting(new Setting(Key, Value));
        }
        public void AddSetting(Setting SettingToAdd)
        {
            this.Settings.Add(SettingToAdd);
        }
        public void RemoveSetting(string Key)
        {
            this.Settings.Remove(Settings.Find(x => x.Key == Key));
        }
        public void SetSettingValue(string Key, string NewValue)
        {
            this.Settings.Find(x => x.Key == Key).ChangeValue(NewValue);
        }
        public void SetSettingKey(string Key, string NewKey)
        {
            this.Settings.Find(x => x.Key == Key).ChangeKey(NewKey);
        }
        public Setting GetSetting(string Key, string Value)
        {
            return Settings.Find(x => x.Key == Key && x.Value == Value);
        }
        public Setting GetSettingByKey(string Key)
        {
            return Settings.Find(x => x.Key == Key);
        }
        public Setting GetSettingByValue(string Value)
        {
            return Settings.Find(x => x.Value == Value);
        }
    }
    public class Setting
    {
        public string Key { get; private set; }
        public string Value { get; private set; }
        public Setting(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
        public void ChangeKey(string NewKey)
        {
            this.Key = NewKey;
        }
        public void ChangeValue(string NewValue)
        {
            this.Key = NewValue;
        }
    }
}
