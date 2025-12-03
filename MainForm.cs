using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeTweakrsNetCleaner
{
    public partial class MainForm : Form
    {
        private readonly CleanerEngine cleaner = new();
        private Button btnAnalyze = new();
        private Button btnClean = new();
        private ProgressBar progressBar = new();
        private Label lblStatus = new();
        private ListBox logBox = new();

        public MainForm()
        {
            Text = "CT-Cleaner Pro | CodeTweakrs";
            Size = new Size(820, 520);
            StartPosition = FormStartPosition.CenterScreen;
            ApplyDarkTheme();
            InitializeUI();
            Load += async (_, _) => await InitializeCleaner();
        }

        private void ApplyDarkTheme()
        {
            BackColor = Color.FromArgb(24, 24, 24);
            ForeColor = Color.WhiteSmoke;
        }

        private void InitializeUI()
        {
            btnAnalyze = new Button
            {
                Text = "Analyze",
                Location = new Point(20, 20),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat
            };
            btnAnalyze.FlatAppearance.BorderColor = Color.Orange;
            btnAnalyze.Click += async (_, _) => await AnalyzeAsync();

            btnClean = new Button
            {
                Text = "Clean",
                Location = new Point(150, 20),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat
            };
            btnClean.FlatAppearance.BorderColor = Color.Orange;
            btnClean.Click += async (_, _) => await CleanAsync();

            logBox = new ListBox
            {
                Location = new Point(20, 80),
                Size = new Size(760, 350),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 440),
                Size = new Size(760, 10),
                Style = ProgressBarStyle.Continuous
            };

            lblStatus = new Label
            {
                Text = "Idle",
                Location = new Point(20, 460),
                AutoSize = true
            };

            Controls.AddRange(new Control[] { btnAnalyze, btnClean, logBox, progressBar, lblStatus });
        }

        private async Task InitializeCleaner()
        {
            try
            {
                await cleaner.InitializeAsync();
                logBox.Items.Add("Ready. Config loaded.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AnalyzeAsync()
        {
            logBox.Items.Add("Analyzing junk paths...");
            lblStatus.Text = "Analyzing...";
            var progress = new Progress<string>(msg => logBox.Items.Add(msg));
            await cleaner.AnalyzeAsync(progress);
            lblStatus.Text = "Analysis complete.";
        }

        private async Task CleanAsync()
        {
            logBox.Items.Add("Cleaning junk files...");
            lblStatus.Text = "Cleaning...";
            var progress = new Progress<string>(msg => logBox.Items.Add(msg));
            await cleaner.CleanAsync(progress);
            lblStatus.Text = "Cleanup complete.";
        }
    }
}
