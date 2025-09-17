using NetSpotifyDownloaderConsole.Utils;

internal class Program
{
    private const string EXIT_OPTION = "3";

    private static void Main(string[] args)
    {
        var settings = SettingsManager.Load();

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

                    break;
                case "2":
                    Console.Write("Nombre de usuario: ");
                    settings.SpotifyClientId = Console.ReadLine();
                    Console.Write($"Cambiado nombre de usuario a {settings.SpotifyClientId}.\n" +
                        "Presione una tecla para continuar.");
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

