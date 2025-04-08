using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using System.Diagnostics;

namespace NetSpotifyDownloaderCore.Repositories.Implementation
{
    public class YoutubeDownloaderRepository : IYoutubeDownloaderRepository
    {
        public async Task<YoutubeDownloadDTO?> GetMp3DownloadUrlAsync(string youtubeUrl)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--extract-audio --audio-format mp3 -f bestaudio --get-url {youtubeUrl}",
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

            // Verificamos si la salida contiene una URL válida
            var url = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (url != null && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var result))
                return new()
                {
                    Uri = new Uri(result.ToString())
                };

            return null;
        }
    }
}
