using NetSpotifyDownloaderCore.Repositories.Interfaces;
using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;

namespace NetSpotifyDownloaderCore.Services
{
    public class SpotifyService
    {
        private readonly ISpotifyRepository _spotifyRepository;

        public SpotifyService(ISpotifyRepository spotifyRepository)
        {
            _spotifyRepository = spotifyRepository;
        }

        public async Task<List<SpotifyPlaylistDTO>> GetUserPlaylistsAsync(string userId)
        {
            var playlists = await _spotifyRepository.GetUserPlaylistsAsync(userId);
            return playlists;
        }

        public async Task<List<SpotifyTrackDTO>> GetTracksByPlaylistAsync(string playlistId)
        {
            var tracks = await _spotifyRepository.GetTracksByPlaylistAsync(playlistId);
            return tracks;
        }
    }
}
