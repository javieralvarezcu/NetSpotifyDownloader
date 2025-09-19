using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;
using System.Linq;

namespace NetSpotifyDownloaderConsole.Model
{
    public class SpotifyTrack : SpotifyTrackDTO
    {
        public YoutubeMusicTrack? YoutubeTrack { get; set; }
        public string ArtistsString { get => string.Join(' ', Artists); }
    }
}
