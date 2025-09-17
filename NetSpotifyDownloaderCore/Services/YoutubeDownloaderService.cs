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
        public async Task<Stream?> GetAudioStreamAsync(string youtubeUrl)
        {
            return await _youtubeDownloaderRepository.GetAudioStreamAsync(youtubeUrl);
        }
    }
}
