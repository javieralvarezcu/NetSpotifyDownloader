using NetSpotifyDownloaderCore.Services;
using System;
using System.Net;
using System.Windows.Forms;

namespace NetSpotifyDownloaderWinForms
{
    public partial class Form1 : Form
    {
        private readonly SpotifyService _spotifyService;
        private readonly HttpClient _httpClient;
        private FlowLayoutPanel flowPanel;
        public Form1(SpotifyService spotifyService, HttpClient httpClient)
        {
            _spotifyService = spotifyService;

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
                        playlist.Thumbnail
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

        private async Task<Control> CreatePlaylistCard(string title, string author, string tracks, Uri? thumbnailUri = null)
        {
            var panel = new Panel
            {
                Width = 180,
                Height = 200,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 40)
            };

            
            var pic = new PictureBox
            {
                Width = 180,
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.Gray, // simulando portada
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

            return panel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
