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
                    var playlists = await http.GetFromJsonAsync<SpotifyPlaylistDTO[]>(
                        $"api/Spotify/{settings.SpotifyClientId}/playlists");

                    if (playlists?.Count() > 0 )
                    {
                        Console.WriteLine("\nPlaylists encontradas:");
                        foreach (var pl in playlists)
                        {
                            Console.WriteLine($"- {pl.Name}");

                            //var tracks = await http.GetFromJsonAsync<List<SpotifyTrackDTO>>(
                            //    $"api/Spotify/{pl.Id}/tracks");
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

