namespace NetSpotifyDownloaderCore.Model.Spotify.DTOs
{
    public class SpotifyTrackDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public int Year { get; set; }
        public string AlbumName { get; set; }
        public Uri AlbumImageUri { get; set; }

    }
}
