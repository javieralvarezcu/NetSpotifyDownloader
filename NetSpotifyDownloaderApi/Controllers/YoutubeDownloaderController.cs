using Microsoft.AspNetCore.Mvc;
using NetSpotifyDownloaderCore.Services;

namespace NetYoutubeDownloaderDownloaderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeDownloaderController : ControllerBase
    {
        private readonly YoutubeDownloaderService _youtubeDownloaderService;

        public YoutubeDownloaderController(YoutubeDownloaderService youtubeDownloaderService)
        {
            _youtubeDownloaderService = youtubeDownloaderService;
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetMp3DownloadUrl([FromQuery] string youtubeUrl)
        {
            var youtubeTrack = await _youtubeDownloaderService.GetMp3DownloadUrlAsync(youtubeUrl);
            return Ok(youtubeTrack);
        }
    }
}
