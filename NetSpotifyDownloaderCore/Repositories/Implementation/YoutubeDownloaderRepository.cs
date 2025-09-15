using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetSpotifyDownloaderCore.Repositories.Implementation
{
    public class YoutubeDownloaderRepository : IYoutubeDownloaderRepository
    {
        private string GetYtDlpPath()
        {
            var basePath = AppContext.BaseDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(basePath, "Tools", "yt-dlp.exe");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Path.Combine(basePath, "Tools", "yt-dlp_linux");

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //    return Path.Combine(basePath, "Tools", "yt-dlp_macos");

            throw new PlatformNotSupportedException("Sistema operativo no soportado");
        }

        public async Task<Stream?> GetAudioStreamAsync(string youtubeUrl)
        {
            var ytDlpPath = GetYtDlpPath();
            var tempFile = Path.GetTempFileName() + ".mp3";

            var psi = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = $"-f bestaudio -x --audio-format mp3 --audio-quality best --embed-metadata --embed-thumbnail -o \"{tempFile}\" {youtubeUrl}",
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi)!;
            await process.WaitForExitAsync();

            if (File.Exists(tempFile))
            {
                return new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Delete, 4096, FileOptions.DeleteOnClose);
            }

            return null;
        }
    }
}
