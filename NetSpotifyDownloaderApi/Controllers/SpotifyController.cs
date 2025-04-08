using Microsoft.AspNetCore.Mvc;
using NetSpotifyDownloaderCore.Services;

namespace NetSpotifyDownloaderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly SpotifyService _spotifyService;

        public SpotifyController(SpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        [HttpGet("{userId}/playlists")]
        public async Task<IActionResult> GetUserPlaylists(string userId)
        {
            var playlists = await _spotifyService.GetUserPlaylistsAsync(userId);
            return Ok(playlists);
        }

        [HttpGet("{playlistId}/tracks")]
        public async Task<IActionResult> GetTracksByPlaylist(string playlistId)
        {
            var tracks = await _spotifyService.GetTracksByPlaylistAsync(playlistId);
            return Ok(tracks);
        }
    }
}
