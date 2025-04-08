using NetSpotifyDownloaderCore.Model.Spotify.DTOs;

namespace NetSpotifyDownloaderCore.Repositories.Interfaces
{
    public interface IYoutubeDownloaderRepository
    {
        Task<YoutubeDownloadDTO?> GetMp3DownloadUrlAsync(string youtubeUrl);
    }
}
