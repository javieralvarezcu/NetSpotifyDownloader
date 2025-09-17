using NetSpotifyDownloaderCore.Options;
using NetSpotifyDownloaderCore.Repositories.Implementation;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using NetSpotifyDownloaderCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SpotifyApiOptions>(
    builder.Configuration.GetSection("Spotify"));

#region Repos
builder.Services.AddHttpClient<ISpotifyRepository, SpotifyApiRepository>();
builder.Services.AddHttpClient<IYoutubeMusicRepository, YoutubeMusicRepository>();
builder.Services.AddScoped<IYoutubeDownloaderRepository, YoutubeDownloaderRepository>();
#endregion

#region Services
builder.Services.AddScoped<SpotifyService>();
builder.Services.AddScoped<YoutubeMusicService>();
builder.Services.AddScoped<YoutubeDownloaderService>();
var app = builder.Build();
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
