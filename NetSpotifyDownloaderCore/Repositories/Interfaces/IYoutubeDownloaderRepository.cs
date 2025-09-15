namespace NetSpotifyDownloaderCore.Repositories.Interfaces
{
    public interface IYoutubeDownloaderRepository
    {
        Task<Stream?> GetAudioStreamAsync(string youtubeUrl);
    }
}
