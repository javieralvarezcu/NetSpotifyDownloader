using Microsoft.AspNetCore.Mvc;
using NetSpotifyDownloaderCore.Services;

namespace NetSpotifyDownloaderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeMusicController : ControllerBase
    {
        private readonly YoutubeMusicService _youtubeMusicService;

        public YoutubeMusicController(YoutubeMusicService YoutubeMusicService)
        {
            _youtubeMusicService = YoutubeMusicService;
        }

        [HttpGet("track")]
        public async Task<IActionResult> GetYoutubeTrack([FromQuery] string title, [FromQuery] string artistName)
        {
            var youtubeTrack = await _youtubeMusicService.SearchTrackAsync(title, artistName);
            return Ok(youtubeTrack);
        }
    }
}
