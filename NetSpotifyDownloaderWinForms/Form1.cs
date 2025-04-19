using NetSpotifyDownloaderCore.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace NetSpotifyDownloaderWinForms
{
    public partial class Form1 : Form
    {
        private readonly SpotifyService _spotifyService;
        private readonly YoutubeMusicService _youtubeMusicService;
        private readonly YoutubeDownloaderService _youtubeDownloaderService;
        private readonly HttpClient _httpClient;
        private FlowLayoutPanel flowPanel;

        private string tempRootPath = @"D:\test spoti";
        public Form1(SpotifyService spotifyService, YoutubeMusicService youtubeMusicService, YoutubeDownloaderService youtubeDownloaderService, HttpClient httpClient)
        {
            _spotifyService = spotifyService;
            _youtubeMusicService = youtubeMusicService;
            _youtubeDownloaderService = youtubeDownloaderService;

            InitializeComponent();

            InitUI();

            // Ejecutamos la carga de playlists en segundo plano una vez todo está montado
            this.Shown += async (_, _) => await LoadPlaylistsAsync(flowPanel);
            _httpClient = httpClient;
        }

        private void InitUI()
        {
            this.Text = "dlu - Music Downloader";
            this.Size = new Size(1200, 800);
            this.BackColor = Color.FromArgb(20, 20, 30);

            // Panel lateral
            var sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = Color.FromArgb(18, 18, 28)
            };
            this.Controls.Add(sidePanel);

            string[] navItems = { "Home", "Search", "Charts", "Favorites", "Link Analyzer", "Settings", "About" };
            int topOffset = 20;

            foreach (var text in navItems)
            {
                var btn = new Button
                {
                    Text = text,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    Width = sidePanel.Width,
                    Height = 40,
                    Top = topOffset,
                    Left = 0,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    Font = new Font("Segoe UI", 10)
                };
                btn.FlatAppearance.BorderSize = 0;
                sidePanel.Controls.Add(btn);
                topOffset += 45;
            }

            // Barra de búsqueda
            var searchBox = new TextBox
            {
                PlaceholderText = "Search anything you want (or just paste a link)",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(35, 35, 50),
                BorderStyle = BorderStyle.None,
                Width = 800,
                Height = 30,
                Top = 20,
                Left = 200
            };
            this.Controls.Add(searchBox);

            // Label principal
            var title = new Label
            {
                Text = "Favorites",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Top = 70,
                Left = 200
            };
            this.Controls.Add(title);

            // Tabs: Playlists, Albums, etc.
            var tabControl = new TabControl
            {
                Top = 120,
                Left = 200,
                Width = 980,
                Height = 600,
                Appearance = TabAppearance.FlatButtons,
                DrawMode = TabDrawMode.OwnerDrawFixed
            };

            tabControl.DrawItem += (s, e) =>
            {
                var tab = ((TabControl)s).TabPages[e.Index];
                var rect = e.Bounds;
                var isSelected = (tabControl.SelectedIndex == e.Index);
                var color = isSelected ? Brushes.White : Brushes.Gray;
                e.Graphics.DrawString(tab.Text, new Font("Segoe UI", 10, FontStyle.Bold), color, rect);
            };

            var playlistsTab = new TabPage("Playlists") { BackColor = Color.FromArgb(20, 20, 30) };

            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            playlistsTab.Controls.Add(flowPanel);

            tabControl.TabPages.Add(playlistsTab);
            this.Controls.Add(tabControl);
        }

        private async Task DownloadPlaylistAsync(string playlistId, string title)
        {
            try
            {
                var tracks = await _spotifyService.GetTracksByPlaylistAsync(playlistId);

                foreach (var track in tracks)
                {
                    var youtubeTrack = await _youtubeMusicService.SearchTrackAsync(track.Title, string.Join(", ", track.Artists));
                    if (youtubeTrack == null)
                    {
                        continue;
                    }

                    var youtubeDownload = await _youtubeDownloaderService.GetMp3DownloadUrlAsync(youtubeTrack.Uri.ToString());
                    if (youtubeDownload == null)
                    {
                        continue;
                    }
                    var downloadUrl = youtubeDownload.Uri.ToString();
                    var fileName = $"{string.Join(", ", track.Artists)} - {track.Title}.mp3".Replace(",  -", " -");
                    var filePath = Path.Combine(tempRootPath, title, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
                    using (var webClient = new WebClient())
                    {
                        var tempFilePath = Path.Combine(tempRootPath, title, $"{Guid.NewGuid()}.weba");
                        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath) ?? string.Empty);

                        await webClient.DownloadFileTaskAsync(new Uri(downloadUrl), tempFilePath);

                         var mp3FilePath = Path.Combine(tempRootPath, title, fileName);

                        // Convert to MP3 using ffmpeg
                        var ffmpeg = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = $"-y -i \"{tempFilePath}\" -vn -ab 192k -ar 44100 -f mp3 \"{mp3FilePath}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (var process = Process.Start(ffmpeg))
                        {
                            if (process == null)
                                throw new InvalidOperationException("Failed to start ffmpeg process");

                            string output = await process.StandardOutput.ReadToEndAsync();
                            string error = await process.StandardError.ReadToEndAsync();

                            await process.WaitForExitAsync();

                            if (process.ExitCode != 0)
                                throw new Exception($"ffmpeg error: {error}");
                        }

                        // Optionally delete the original file
                        File.Delete(tempFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando canciones: " + ex.Message);
            }
        }

        private async Task LoadPlaylistsAsync(FlowLayoutPanel flowPanel)
        {
            // Mostrar placeholders mientras carga
            for (int i = 0; i < 8; i++)
            {
                var card = await CreatePlaylistCard("Cargando...", "by ...", "...");
                flowPanel.Controls.Add(card);
            }

            try
            {
                var playlists = await _spotifyService.GetUserPlaylistsAsync("thejavieralcu99");

                // Reemplazar placeholders
                flowPanel.Controls.Clear();

                foreach (var playlist in playlists)
                {
                    var card = await CreatePlaylistCard(
                        playlist.Name,
                        $"by {playlist.Owner}",
                        $"{playlist.TracksCount} tracks",
                        playlist.Thumbnail,
                        playlist.Id
                    );

                    flowPanel.Controls.Add(card);

                    // Permitir a la UI actualizarse entre cada render
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando playlists: " + ex.Message);
            }
        }

        private async Task<Control> CreatePlaylistCard(string title, string author, string tracks, Uri? thumbnailUri = null, string? playlistId = null)
        {
            var panel = new Panel
            {
                Width = 180,
                Height = 200,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 40),
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Width = 180,
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.Gray,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            if (thumbnailUri != null)
            {
                try
                {
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(thumbnailUri);
                    using var ms = new MemoryStream(imageBytes);
                    pic.Image = Image.FromStream(ms);
                }
                catch
                {
                    // Si falla la carga, dejar imagen gris
                }
            }

            var titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(5, 0, 5, 0)
            };

            var authorLabel = new Label
            {
                Text = author,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 20,
                Padding = new Padding(5, 0, 5, 0)
            };

            var trackLabel = new Label
            {
                Text = tracks,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8),
                Dock = DockStyle.Top,
                Height = 20,
                Padding = new Padding(5, 0, 5, 0)
            };

            panel.Controls.Add(trackLabel);
            panel.Controls.Add(authorLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(pic);

            void PanelClickHandler(object? sender, EventArgs e)
            {
                if (playlistId != null)
                {
                    // Limpiar vista actual
                    flowPanel.Controls.Clear();

                    // Botón de volver
                    var backButton = new Button
                    {
                        Text = "⬅ Back to playlists",
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(50, 50, 70),
                        Height = 40,
                        Width = 200,
                        Margin = new Padding(10),
                    };
                    backButton.FlatAppearance.BorderSize = 0;
                    backButton.Click += async (_, _) =>
                    {
                        flowPanel.Controls.Clear();
                        await LoadPlaylistsAsync(flowPanel);
                    };
                    flowPanel.Controls.Add(backButton);

                    var downloadButton = new Button
                    {
                        Text = "Download playlist",
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(50, 50, 70),
                        Height = 40,
                        Width = 200,
                        Margin = new Padding(10),
                    };
                    downloadButton.FlatAppearance.BorderSize = 0;
                    downloadButton.Click += async (_, _) =>
                    {
                        flowPanel.Controls.Clear();
                        _ = DownloadPlaylistAsync(playlistId, title);
                    };
                    flowPanel.Controls.Add(downloadButton);

                    // Título de la playlist
                    var header = new Label
                    {
                        Text = title,
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        ForeColor = Color.White,
                        Height = 50,
                        Width = flowPanel.Width - 40,
                        Padding = new Padding(10),
                    };
                    flowPanel.Controls.Add(header);

                    _ = LoadPlaylistTracksAsync(playlistId);
                }
            }

            panel.Click += PanelClickHandler;
            pic.Click += PanelClickHandler;
            titleLabel.Click += PanelClickHandler;
            authorLabel.Click += PanelClickHandler;
            trackLabel.Click += PanelClickHandler;

            return panel;
        }

        private async Task LoadPlaylistTracksAsync(string playlistId)
        {
            try
            {
                var tracks = await _spotifyService.GetTracksByPlaylistAsync(playlistId);

                foreach (var track in tracks)
                {
                    var row = new Panel
                    {
                        Width = flowPanel.Width - 40,
                        Height = 40,
                        BackColor = Color.FromArgb(25, 25, 35),
                        Margin = new Padding(5),
                        Padding = new Padding(10)
                    };

                    var label = new Label
                    {
                        Text = $"{track.Title} — {string.Join(", ", track.Artists)}",
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10),
                        AutoSize = true
                    };

                    row.Controls.Add(label);
                    flowPanel.Controls.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando canciones: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
