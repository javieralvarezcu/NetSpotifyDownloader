using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSpotifyDownloaderConsole.Model
{
    public class SpotifyPlaylist : SpotifyPlaylistDTO
    {
        public SpotifyTrack[]? SpotifyTracks { get; set; }
    }
}
