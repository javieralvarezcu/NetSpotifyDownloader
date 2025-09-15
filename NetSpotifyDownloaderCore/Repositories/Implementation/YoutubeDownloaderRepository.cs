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

        public async Task<YoutubeDownloadDTO?> GetMp3DownloadUrlAsync(string youtubeUrl)
        {
            var ytDlpPath = GetYtDlpPath();

            var psi = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = @$"--extract-audio --audio-format mp3 -f bestaudio --get-url {youtubeUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                throw new InvalidOperationException("No se pudo lanzar yt-dlp");

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp error: {error}");

            var url = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (url != null && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var result))
                return new()
                {
                    Uri = result
                };

            return null;
        }
    }
}
