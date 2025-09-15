using AngleSharp.Dom;
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
        public async Task<IActionResult> Download([FromQuery] string youtubeUrl)
        {
            var stream = await _youtubeDownloaderService.GetAudioStreamAsync(youtubeUrl);
            if (stream == null)
                return BadRequest("No se pudo obtener el audio");

            // Lo devolvemos como stream HTTP
            return File(stream, "audio/mp3", "cancion.mp3");
        }
    }
}
