using NetSpotifyDownloaderCore.Model.Spotify.DTOs;

namespace NetSpotifyDownloaderCore.Repositories.Interfaces
{
    public interface ISpotifyRepository
    {
        Task<List<SpotifyPlaylistDTO>> GetUserPlaylistsAsync(string userId);

        Task<List<SpotifyTrackDTO>> GetTracksByPlaylistAsync(string playlistId);
    }
}
