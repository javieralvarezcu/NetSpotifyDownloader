using System.Text.Json;

namespace NetSpotifyDownloaderConsole.Utils
{
    public static class SettingsManager
    {
        private static readonly string FilePath = "customappsettings.json";

        public static AppSettings Load()
        {
            if (!File.Exists(FilePath))
                return new AppSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
