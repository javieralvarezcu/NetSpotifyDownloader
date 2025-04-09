namespace NetSpotifyDownloaderCore.Model.Spotify.DTOs
{
    public class SpotifyPlaylistDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public string Owner { get; set; }
        public int TracksCount { get; set; }
        public Uri Thumbnail { get; set; }
    }
}
