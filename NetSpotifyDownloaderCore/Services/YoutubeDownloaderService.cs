using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;

namespace NetSpotifyDownloaderCore.Services
{
    public class YoutubeDownloaderService
    {
        private readonly IYoutubeDownloaderRepository _youtubeDownloaderRepository;
        public YoutubeDownloaderService(IYoutubeDownloaderRepository youtubeDownloaderRepository)
        {
            _youtubeDownloaderRepository = youtubeDownloaderRepository;
        }
        public async Task<YoutubeDownloadDTO?> GetMp3DownloadUrlAsync(string youtubeUrl)
        {
            return await _youtubeDownloaderRepository.GetMp3DownloadUrlAsync(youtubeUrl);
        }
    }
}
