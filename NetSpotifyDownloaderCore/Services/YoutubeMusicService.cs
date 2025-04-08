using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;

namespace NetSpotifyDownloaderCore.Services
{
    public class YoutubeMusicService
    {
        private readonly IYoutubeMusicRepository _youtubeMusicRepository;

        public YoutubeMusicService(IYoutubeMusicRepository youtubeMusicRepository)
        {
            _youtubeMusicRepository = youtubeMusicRepository;
        }

        public async Task<YoutubeMusicTrackDTO?> SearchTrackAsync(string title, string artistName)
        {
            var track = await _youtubeMusicRepository.SearchTrackAsync(title, artistName);
            return track;
        }
    }
}
