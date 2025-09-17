using System;
using System.Net.Http;
using System.Threading.Tasks;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using NetSpotifyDownloaderDomain.Model.Spotify.DTOs;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--headless=new"
        };

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                         "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                         "Chrome/122.0.0.0 Safari/537.36";

        public YoutubeMusicRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<YoutubeMusicTrackDTO?> SearchTrackAsync(string title, string artistName)
        {
            var options = new ChromeOptions();
            options.AddArguments(BrowserArgs);
            options.AddArgument($"--user-agent={UserAgent}");

            using var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //driver.Manage().Cookies.AddCookie(new Cookie("CONSENT", "YES+cb"));

            var query = Uri.EscapeDataString($"{title} {artistName}");
            var searchUrl = $"https://music.youtube.com/search?q={query}";
            driver.Navigate().GoToUrl(searchUrl);
            Thread.Sleep(1000);

            // Fake webdriver flag
            ((IJavaScriptExecutor)driver).ExecuteScript("Object.defineProperty(navigator, 'webdriver', { get: () => false });");
            Thread.Sleep(1000);

            AcceptConsentIfNeeded(driver);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            try
            {
                // 1. Intentar localizar el bloque "Mejor resultado"
                var bestResultSection = wait.Until(drv => drv.FindElement(By.XPath(
                    "//ytmusic-card-shelf-renderer[.//yt-formatted-string[text()='Mejor resultado' or text()='Top result']]"
                )));

                // 2. Verificar si el "Mejor resultado" es una canción
                var subtitle = bestResultSection.FindElement(By.CssSelector(".subtitle")).Text.ToLower();

                if (subtitle.Contains("canción") || subtitle.Contains("song"))
                {
                    // ✅ Es canción → devolvemos ese enlace
                    var bestResultLink = bestResultSection.FindElement(By.CssSelector("a.yt-simple-endpoint[href*='watch']"));
                    var href = bestResultLink.GetAttribute("href");

                    return href != null
                        ? new YoutubeMusicTrackDTO { Uri = new Uri(href) }
                        : null;
                }
            }
            catch
            {
                // Ignoramos y seguimos buscando en "Canciones"
            }

            try
            {
                // 3. Si el "Mejor resultado" no era canción → buscamos en "Canciones"
                var songsSection = wait.Until(drv => drv.FindElement(By.XPath(
                    "//ytmusic-shelf-renderer[.//yt-formatted-string[text()='Canciones' or text()='Songs']]"
                )));

                var firstSongLink = songsSection.FindElement(By.CssSelector("ytmusic-responsive-list-item-renderer a.yt-simple-endpoint[href*='watch']"));
                var href = firstSongLink.GetAttribute("href");

                return href != null
                    ? new YoutubeMusicTrackDTO { Uri = new Uri(href) }
                    : null;
            }
            catch
            {
                throw new Exception("No se pudo encontrar una canción en los resultados de YouTube Music");
            }
        }

        private static void AcceptConsentIfNeeded(IWebDriver driver)
        {
            if (!driver.Url.Contains("consent.youtube.com"))
                return;

            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var acceptButton = wait.Until(drv =>
    drv.FindElement(By.XPath("//button[contains(., 'Aceptar')] | //button[contains(., 'Accept all')]"))
);
                acceptButton.Click();

                // Esperar redirección a la búsqueda
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(drv => drv.Url.Contains("/search"));
            }
            catch
            {
                throw new Exception("No se pudo aceptar el consentimiento de cookies.");
            }
        }
    }
}