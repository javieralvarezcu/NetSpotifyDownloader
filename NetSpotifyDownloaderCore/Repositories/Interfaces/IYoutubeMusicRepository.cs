using NetSpotifyDownloaderCore.Model.Spotify.DTOs;

namespace NetSpotifyDownloaderCore.Repositories.Interfaces
{
    public interface IYoutubeMusicRepository
    {
        public Task<YoutubeMusicTrackDTO?> SearchTrackAsync(string title, string artistName);
    }
}
