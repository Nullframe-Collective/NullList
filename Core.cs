using Il2Cpp;
using Il2CppCodeStage.AntiCheat.Storage;
using Il2CppFishNet;
using Il2CppFishNet.Managing.Server;
using MelonLoader;

[assembly: MelonInfo(typeof(NullList.Core), "NullList", "1.0.0", "Nullframe Collective", null)]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]
[assembly: MelonColor(1, 255, 255, 255)]

namespace NullList
{
    public class Core : MelonMod
    {
        public static string Blacklist;
        public static bool IsHost;
        private Config _config;
        private string _nickName;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            _config = ConfigManager.Load();

            if (_config.AutoUpdate)
            {
                ModUpdater.CheckForUpdates(this, "Nullframe-Collective", "NullList");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu" || sceneName == "Updater")
            {
                SetupConfigAsync().Wait();
                _nickName = ObscuredPrefs.GetString("ZWName0001");
            }

            if (RoomProperties.Current != null)
            {
                IsHost = RoomProperties.Current.HostName == _nickName ? true : false;
            }
        }

        public static void Log(object msg)
        {
            MelonLogger.Msg(msg);
        }

        private async Task SetupConfigAsync()
        {
            if (!File.Exists(_config.LocalBlacklistPath))
            {
                File.Create(_config.LocalBlacklistPath).Dispose();

                return;
            }

            Blacklist = File.ReadAllText(_config.LocalBlacklistPath);

            if (_config.GlobalBlacklist)
            {
                Blacklist = await DownloadFile(_config.LinkToGlobalBlacklist);
            }
        }

        private static async Task<string> DownloadFile(string uri)
        {
            using (HttpClient httpClient = new())
            {
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uri);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        public static bool CheckPlayer(string nickName)
        {
            if (string.IsNullOrEmpty(Blacklist)) return false;

            string[] blacklist = Blacklist.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

            foreach (string player in blacklist)
            {
                if (player.Equals(nickName, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        public static void Kick(PlayerInfo playerInfo)
        {
            InstanceFinder.ServerManager.Kick(playerInfo.OwnerId, KickReason.Unset);
        }
    }
}