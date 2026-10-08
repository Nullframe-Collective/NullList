using System.Text.Json;

namespace NullList
{
    public struct Config
    {
        public Config()
        {
            AutoUpdate = true;
            GlobalBlacklist = true;
            LinkToGlobalBlacklist = "https://raw.githubusercontent.com/Nullframe-Collective/Nullframe-Blacklist/refs/heads/main/Blacklist.txt";
            LocalBlacklistPath = "UserData/NullList/Blacklist.txt";
        }

        public bool AutoUpdate { get; set; }
        public bool GlobalBlacklist { get; set; }
        public string LinkToGlobalBlacklist { get; set; }
        public string LocalBlacklistPath { get; set; }
    }

    public static class ConfigManager
    {
        private const string ConfigPath = "UserData/NullList/Config.json";

        public static Config Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);

                    return JsonSerializer.Deserialize<Config?>(json) ?? new Config();
                }
                catch { }
            }

            return Save(new Config());
        }

        public static Config Save(Config config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);

            return config;
        }
    }
}
