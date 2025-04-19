using NetSpotifyDownloaderCore.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSpotifyDownloaderWinForms
{
    public partial class PlaylistDetailsForm : Form
    {
        private readonly SpotifyService _spotifyService;
        private readonly string _playlistId;

        public PlaylistDetailsForm(SpotifyService spotifyService, string playlistId, string playlistName)
        {
            _spotifyService = spotifyService;
            _playlistId = playlistId;

            InitializeComponent();
            InitUI(playlistName);
            LoadTracksAsync();
        }

        private void InitUI(string playlistName)
        {
            this.Text = playlistName;
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(20, 20, 30);

            var titleLabel = new Label
            {
                Text = playlistName,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            this.Controls.Add(flowPanel);

            this.Tag = flowPanel;
        }

        private async void LoadTracksAsync()
        {
            var flowPanel = (FlowLayoutPanel)this.Tag;

            try
            {
                var tracks = await _spotifyService.GetTracksByPlaylistAsync(_playlistId);

                foreach (var track in tracks)
                {
                    var trackLabel = new Label
                    {
                        Text = $"{track.Title} - {string.Join(", ", track.Artists)}",
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10),
                        AutoSize = true,
                        Padding = new Padding(5)
                    };
                    flowPanel.Controls.Add(trackLabel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando canciones: " + ex.Message);
            }
        }
    }
}
