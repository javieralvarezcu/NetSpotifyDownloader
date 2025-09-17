using Microsoft.Extensions.Options;
using NetSpotifyDownloaderCore.Exceptions;
using NetSpotifyDownloaderCore.Options;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;
using NetSpotifyDownloaderDomain.Model.Spotify.Repository;
using System.Text;
using System.Text.Json;

namespace NetSpotifyDownloaderCore.Repositories.Implementation
{
    public class SpotifyApiRepository : ISpotifyRepository
    {
        private readonly HttpClient _httpClient;
        private readonly SpotifyApiOptions _options;

        public SpotifyApiRepository(HttpClient httpClient, IOptions<SpotifyApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<List<SpotifyTrackDTO>> GetTracksByPlaylistAsync(string playlistId)
        {
            try
            {
                var url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit=100";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                string accessToken = await GetAccessTokenAsync();
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpotifyApiException($"Spotify API devolvió código {response.StatusCode}: {content}");
                }

                var playlistResponse = JsonSerializer.Deserialize<PlaylistResponse>(content);

                if (playlistResponse?.items == null)
                {
                    throw new SpotifyApiException("No se pudieron deserializar las canciones de la playlist.");
                }

                return playlistResponse.items
                    .Where(i => i.track != null)
                    .Select(i => new SpotifyTrackDTO
                    {
                        Id = i.track.id,
                        Title = i.track.name,
                        Artists = i.track.artists?.Select(a => a.name).ToArray() ?? Array.Empty<string>(),
                        Year = i.track.album?.release_date != null ? DateTime.Parse(i.track.album.release_date).Year : 0,
                        AlbumName = i.track.album?.name ?? "Desconocido",
                        AlbumImageUri = GetBestImageUri(i.track.album?.images)
                    })
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new SpotifyApiException("Error de conexión al obtener canciones de la playlist.", ex);
            }
            catch (JsonException ex)
            {
                throw new SpotifyApiException("Error al deserializar la respuesta JSON de tracks.", ex);
            }
            catch (Exception ex)
            {
                throw new SpotifyApiException("Error inesperado al obtener canciones de la playlist.", ex);
            }
        }

        public async Task<List<SpotifyPlaylistDTO>> GetUserPlaylistsAsync(string userId)
        {
            try
            {
                string accessToken = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/users/{userId}/playlists?limit=50");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new SpotifyApiException($"Spotify API devolvió código {response.StatusCode}: {content}");
                }

                var deserialized = JsonSerializer.Deserialize<UserDataResponse>(content);

                if (deserialized == null || deserialized.items == null)
                {
                    throw new SpotifyApiException("No se pudo deserializar la respuesta de playlists de Spotify.");
                }

                List<SpotifyPlaylistDTO> playlists = new List<SpotifyPlaylistDTO>();

                return deserialized.items
                    .Select(p => new SpotifyPlaylistDTO
                    {
                        Id = p.id,
                        Name = p.name,
                        Uri = new Uri(p.external_urls.spotify),
                        Owner = p.owner.display_name,
                        TracksCount = p.tracks.total,
                        Thumbnail = p.images.Any() ? new(p.images.FirstOrDefault().url) : new Uri("https://openverse.org/_nuxt/image_not_available_placeholder.BTm11Bgh.png")
                    }).ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new SpotifyApiException("Error de conexión al API de Spotify.", ex);
            }
            catch (JsonException ex)
            {
                throw new SpotifyApiException("Error al deserializar la respuesta JSON.", ex);
            }
            catch (Exception ex)
            {
                throw new SpotifyApiException("Error inesperado al obtener playlists de Spotify.", ex);
            }
        }

        private static Uri GetBestImageUri(Album_Image[]? images)
        {
            var imageUrl = images?
                .OrderByDescending(i => i.width)
                .FirstOrDefault()?.url;

            return imageUrl != null ? new Uri(imageUrl) : new Uri("https://openverse.org/_nuxt/image_not_available_placeholder.BTm11Bgh.png");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            using var client = new HttpClient();
            var authString = $"{_options.ClientId}:{_options.ClientSecret}";
            var authBytes = Encoding.UTF8.GetBytes(authString);
            var authBase64 = Convert.ToBase64String(authBytes);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authBase64);

            var requestBody = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("grant_type", "client_credentials")
    });

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new SpotifyApiException($"Error al obtener token de acceso: {response.StatusCode}: {content}");
            }

            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("access_token").GetString()
                   ?? throw new SpotifyApiException("La respuesta no contiene un token de acceso.");
        }

        private async Task GetPlaylistTracks(string playlistId, string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit=100";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);

            Console.WriteLine($"\nCanciones en la playlist {playlistId}:\n");

            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var track = item.GetProperty("track");
                    string name = track.GetProperty("name").GetString();
                    string artist = track.GetProperty("artists")[0].GetProperty("name").GetString();
                    Console.WriteLine($"- {artist} – {name}");
                }
            }
            else
            {
                Console.WriteLine("No se pudieron obtener las canciones.");
                Console.WriteLine(content);
            }
        }
    }
}
