using Microsoft.Playwright;
using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;

namespace NetSpotifyDownloaderCore.Repositories.Implementation
{
    public class YoutubeMusicRepository : IYoutubeMusicRepository
    {
        private readonly HttpClient _httpClient;

        private static readonly string[] BrowserArgs = new[]
        {
            "--disable-blink-features=AutomationControlled",
            "--disable-infobars",
            "--no-sandbox",
            "--disable-dev-shm-usage"
        };

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                                         "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        public YoutubeMusicRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<YoutubeMusicTrackDTO?> SearchTrackAsync(string title, string artistName)
        {
            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = BrowserArgs
            }).ConfigureAwait(false);

            var context = await CreateContextAsync(browser).ConfigureAwait(false);
            var page = await context.NewPageAsync().ConfigureAwait(false);

            var query = Uri.EscapeDataString($"{title} {artistName}");
            var searchUrl = $"https://music.youtube.com/search?q={query}";

            await page.GotoAsync(searchUrl, new() { WaitUntil = WaitUntilState.NetworkIdle }).ConfigureAwait(false);
            await page.EvaluateAsync("() => { Object.defineProperty(navigator, 'webdriver', { get: () => false }); }").ConfigureAwait(false);

            await AcceptConsentIfNeededAsync(page).ConfigureAwait(false);

            var trackSection = page
                .Locator("ytmusic-shelf-renderer:has(h2:has-text('Canciones'))")
                .First;

            await trackSection.WaitForAsync().ConfigureAwait(false);

            var href = await trackSection
                .Locator("ytmusic-responsive-list-item-renderer a[href^='watch']")
                .First
                .GetAttributeAsync("href").ConfigureAwait(false);

            return href != null
                ? new YoutubeMusicTrackDTO { Uri = new Uri($"https://music.youtube.com/{href}") }
                : null;
        }

        private async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
        {
            return await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = UserAgent,
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                Locale = "es-ES",
                ColorScheme = ColorScheme.Light,
                JavaScriptEnabled = true
            }).ConfigureAwait(false);
        }

        private static async Task AcceptConsentIfNeededAsync(IPage page)
        {
            if (!page.Url.Contains("consent.youtube.com"))
                return;

            try
            {
                var acceptButton = page.Locator("button[aria-label='Aceptar todo']").First;
                await acceptButton.WaitForAsync(new() { Timeout = 5000 }).ConfigureAwait(false);
                await acceptButton.ClickAsync().ConfigureAwait(false);
                await page.WaitForURLAsync("**/search**", new() { Timeout = 10000 }).ConfigureAwait(false);
            }
            catch
            {
                throw new Exception("No se pudo aceptar el consentimiento de cookies.");
            }
        }
    }
}
