using System;
using System.Net.Http;
using System.Threading.Tasks;
using NetSpotifyDownloaderCore.Model.Spotify.DTOs;
using NetSpotifyDownloaderCore.Repositories.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
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
            "--headless=new" // Headless en Selenium 4.11+
        };

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                         "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                         "Edge/122.0.0.0 Safari/537.36";

        public YoutubeMusicRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<YoutubeMusicTrackDTO?> SearchTrackAsync(string title, string artistName)
        {
            var options = new EdgeOptions();
            options.AddArguments(BrowserArgs);
            options.AddArgument($"--user-agent={UserAgent}");

            using var driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var query = Uri.EscapeDataString($"{title} {artistName}");
            var searchUrl = $"https://music.youtube.com/search?q={query}";
            driver.Navigate().GoToUrl(searchUrl);
            Thread.Sleep(1000);

            // Fake webdriver flag
            ((IJavaScriptExecutor)driver).ExecuteScript("Object.defineProperty(navigator, 'webdriver', { get: () => false });");
            Thread.Sleep(1000);

            AcceptConsentIfNeeded(driver);

            var bestResultSection = driver.FindElement(By.XPath(
    "//ytmusic-card-shelf-renderer[.//h2//yt-formatted-string[contains(text(),'Mejor resultado')]]"
));

            // Buscar el primer enlace dentro de ese bloque
            var bestResultLink = bestResultSection.FindElement(
                By.XPath(".//a[contains(@href,'watch')]")
            );

            var href = bestResultLink.GetAttribute("href");

            return href != null
                ? new YoutubeMusicTrackDTO { Uri = new Uri(href) }
                : null;
        }

        private static void AcceptConsentIfNeeded(IWebDriver driver)
        {
            if (!driver.Url.Contains("consent.youtube.com"))
                return;

            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var acceptButton = wait.Until(drv => drv.FindElement(By.CssSelector("button[aria-label='Aceptar todo']")));
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