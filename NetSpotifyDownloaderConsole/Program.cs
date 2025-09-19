using NetSpotifyDownloaderConsole.Model;
using NetSpotifyDownloaderConsole.Utils;
using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;
using System.Net.Http.Json;

internal class Program
{
    private const string EXIT_OPTION = "3";

    private static async Task Main(string[] args)
    {
        var settings = SettingsManager.Load();
        using var http = new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl) };

        Console.WriteLine(settings.ApiBaseUrl);
        Console.WriteLine(settings.SpotifyClientId);

        string? option = "0";
        do
        {
            Console.WriteLine("Selecciona una opción:\n" +
            $"1. Descargar playlists de {settings.SpotifyClientId}\n" +
            $"2. Cambiar usuario de spotify\n" +
            $"3. Salir");
            option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    List<SpotifyPlaylist> playlistsToDownload = new();
                    SpotifyPlaylist[]? playlists = await http.GetFromJsonAsync<SpotifyPlaylist[]>(
                        $"api/Spotify/{settings.SpotifyClientId}/playlists");

                    if (playlists?.Count() > 0)
                    {
                        Console.WriteLine("\nPlaylists encontradas:");
                        for (int i = 0; i < playlists.Length; i++)
                        {
                            Console.WriteLine($"{i + 1}. {playlists[i].Name}");

                        }
                        Console.WriteLine("\n\nSelecciona playlists separadas por comas o selecciona todas escribiendo con ENTER:");
                        string? selectedPlaylistsString = Console.ReadLine();
                        List<int> selectedPlaylists = new();
                        if (!string.IsNullOrEmpty(selectedPlaylistsString))
                        {
                            selectedPlaylists = selectedPlaylistsString?.Split(',').Select(s => int.Parse(s.Trim()) - 1).ToList();
                        }

                        if (selectedPlaylists.Count == 0)
                        {
                            selectedPlaylists = new();
                            foreach (SpotifyPlaylist playlist in playlists)
                            {
                                selectedPlaylists.Add(playlists.ToList().IndexOf(playlist));
                            }
                        }

                        foreach (var playlistIndex in selectedPlaylists)
                        {
                            var playlist = playlists[playlistIndex];
                            Console.WriteLine($"\nObteniendo canciones de {playlist.Name}");
                            playlists[playlistIndex].SpotifyTracks = await http.GetFromJsonAsync<SpotifyTrack[]>($"api/Spotify/{playlist.Id}/tracks");

                            playlistsToDownload.Add(playlists[playlistIndex]);
                            //playlistsToDownload.Add(playlist, await http.GetFromJsonAsync<SpotifyTrack[]>($"api/Spotify/{playlist.Id}/tracks"));
                        }

                        foreach (var playlist in playlistsToDownload)
                        {
                            foreach(var song in playlist.SpotifyTracks)
                            {
                                Console.WriteLine($"Obteniendo url de youtube de: {song.ArtistsString} - {song.Title}");
                                var url = $"api/YoutubeMusic/track?title={Uri.EscapeDataString(song.Title)}&artistName={Uri.EscapeDataString(song.ArtistsString)}";
                                song.YoutubeTrack = await http.GetFromJsonAsync<YoutubeMusicTrack?>(url);
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("No se encontraron playlists.");
                    }

                    Console.WriteLine("\nPresiona una tecla para continuar...");
                    Console.ReadKey();
                    break;
                case "2":
                    Console.Write("Nombre de usuario: ");

                    settings.SpotifyClientId = Console.ReadLine();
                    SettingsManager.Save(settings);

                    Console.Write($"Cambiado nombre de usuario a {settings.SpotifyClientId}.\n" +
                        "Presiona una tecla para continuar...");
                    Console.ReadKey();
                    break;
                default:
                    break;
            }
            Console.Clear();
        } while (option != EXIT_OPTION);

        Environment.Exit(0);
    }
}

