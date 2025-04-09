using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetSpotifyDownloaderCore.Options;
using NetSpotifyDownloaderCore.Repositories.Implementation;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using NetSpotifyDownloaderCore.Services;
using System.Reflection;

namespace NetSpotifyDownloaderWinForms
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IServiceCollection ServiceCollection { get; set; }


        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddUserSecrets(Assembly.GetExecutingAssembly()); // esto carga el secrets.json
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<ISpotifyRepository, SpotifyApiRepository>();
                    services.AddHttpClient<IYoutubeMusicRepository, YoutubeMusicRepository>();
                    services.AddScoped<IYoutubeDownloaderRepository, YoutubeDownloaderRepository>();

                    var config = context.Configuration;
                    services.Configure<SpotifyApiOptions>(config.GetSection("Spotify"));

                    services.AddScoped<SpotifyService>();
                    services.AddScoped<YoutubeMusicService>();
                    services.AddScoped<YoutubeDownloaderService>();

                    services.AddScoped<Form1>(); // <--- importante
                });

            using var host = builder.Build();

            var form = host.Services.GetRequiredService<Form1>(); // <--- correcto
            Application.Run(form);
        }
    }
}