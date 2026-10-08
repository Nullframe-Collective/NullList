using MelonLoader;
using System.Diagnostics;
using System.Text.Json;

namespace NullList
{
    public static class ModUpdater
    {
        private static readonly HttpClient Client = new HttpClient();

        public static void CheckForUpdates(MelonMod modInstance, string repoOwner, string repoName)
        {
            if (modInstance?.Info == null)
            {
                Core.Log("MelonInfo is null");

                return;
            }

            string modAssemblyPath = modInstance.MelonAssembly.Location;
            _ = FetchAndApplyUpdate(modInstance.Info, modAssemblyPath, repoOwner, repoName);
        }

        private static async Task FetchAndApplyUpdate
        (
            MelonInfoAttribute modInfo,
            string modAssemblyPath,
            string repoOwner,
            string repoName
        )
        {
            try
            {
                Client.DefaultRequestHeaders.UserAgent.ParseAdd($"{modInfo.Name}-Updater");

                string releaseUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
                string responseJson = await Client.GetStringAsync(releaseUrl);
                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    JsonElement root = doc.RootElement;
                    string latestVersionStr = root.GetProperty("tag_name").GetString()?.TrimStart('v');

                    if (!Version.TryParse(latestVersionStr, out Version latestVersion)) return;
                    if (!Version.TryParse(modInfo.Version, out Version currentVersion)) return;
                    if (latestVersion <= currentVersion) return;

                    Core.Log($"New version available {latestVersion}, current: {currentVersion}");

                    JsonElement assets = root.GetProperty("assets");
                    string downloadUrl = null;

                    foreach (JsonElement asset in assets.EnumerateArray())
                    {
                        string name = asset.GetProperty("name").GetString();

                        if (name != null && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = asset.GetProperty("browser_download_url").GetString();

                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        Core.Log($"Not founded DLL");
                        return;
                    }

                    string newPath = modAssemblyPath + ".new";
                    byte[] dllData = await Client.GetByteArrayAsync(downloadUrl);
                    await File.WriteAllBytesAsync(newPath, dllData);

                    Core.Log($"New DLL downloaded");

                    await DownloadAndRunUpdater(repoOwner, repoName, modAssemblyPath, newPath, modInfo.Name);
                }
            }
            catch (Exception ex)
            {
                Core.Log($"Error: {ex.Message}");
            }
        }

        private static async Task DownloadAndRunUpdater
        (
            string repoOwner,
            string repoName,
            string currentPath,
            string newPath,
            string modName
        )
        {
            try
            {
                string updaterUrl = $"https://raw.githubusercontent.com/{repoOwner}/{repoName}/main/Updater.bat";

                Core.Log($"Downloading updater: {updaterUrl}");

                string template = await Client.GetStringAsync(updaterUrl);
                string gameProcessName = Process.GetCurrentProcess().ProcessName;
                string batchContent = template
                    .Replace("{{TARGET}}", currentPath)
                    .Replace("{{NEW}}", newPath)
                    .Replace("{{GAME}}", gameProcessName);

                string batchPath = Path.Combine(Path.GetTempPath(), $"{modName}_Updater.bat");

                Core.Log($"Creating updater: {batchPath}");

                await File.WriteAllTextAsync(batchPath, batchContent);
                if (!File.Exists(batchPath))
                {
                    Core.Log("Failed to create updater file");

                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/d /c call \"{batchPath}\"",
                    WorkingDirectory = Path.GetDirectoryName(batchPath),
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Core.Log($"Starting updater");

                Process updaterProcess = Process.Start(startInfo);
                if (updaterProcess == null)
                {
                    Core.Log("Failed to start updater process");

                    return;
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Core.Log($"Error with updater: {ex}");
            }
        }

        private static string EscapeBatch(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value.Replace("^", "^^")
                        .Replace("&", "^&")
                        .Replace("|", "^|")
                        .Replace("<", "^<")
                        .Replace(">", "^>")
                        .Replace("\"", "\"\"");
        }
    }
}