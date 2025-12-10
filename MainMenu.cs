namespace SpaceRacer
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class MainMenu : Form
    {
        private const string TitleArt = @"--     __   __        __   ___     __        __   ___  __  
--    /__` |__)  /\  /  ` |__     |__)  /\  /  ` |__  |__) 
--    .__/ |    /~~\ \__, |___    |  \ /~~\ \__, |___ |  \ 
--                                                         ";

    private Label titleLabel = null!;
    private Button startButton = null!;
    private Button howToPlayButton = null!;
    private Button exitButton = null!;

        public MainMenu()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Space Racer";
            this.Size = new Size(800, 600);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title Label (large stylized title)
            titleLabel = new Label
            {
                Text = "𝓢𝓹𝓪𝓬𝓮 𝓡𝓪𝓬𝓮𝓻",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                // Use a large, readable font. 'Segoe UI Symbol' has good Unicode support.
                Font = new Font("Segoe UI Symbol", 48, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(760, 110),
                Location = new Point((800 - 760) / 2, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Start Button
            startButton = new Button
            {
                Text = "Start",
                Size = new Size(200, 50),
                Location = new Point(300, 300),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            startButton.Click += StartButton_Click;

            // How to Play Button
            howToPlayButton = new Button
            {
                Text = "How to Play",
                Size = new Size(200, 50),
                Location = new Point(300, 370),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            howToPlayButton.Click += HowToPlayButton_Click;

            // Exit Button
            exitButton = new Button
            {
                Text = "Exit",
                Size = new Size(200, 50),
                Location = new Point(300, 440),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            exitButton.Click += ExitButton_Click;

            // Add hover effects (red theme)
            foreach (Button button in new[] { startButton, howToPlayButton, exitButton })
            {
                button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(255, 70, 70);
                button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(200, 40, 40);
            }

            // Add controls to form
            this.Controls.AddRange(new Control[] { titleLabel, startButton, howToPlayButton, exitButton });
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            this.Hide();
            var gameForm = new GameForm();
            gameForm.ShowDialog(this); // use ShowDialog to wait for game to close
            this.Show(); // show main menu again when game closes
        }

        private void HowToPlayButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "How to Play Space Racer:\n\n" +
                "Controls:\n" +
                "• Arrow Keys or WASD - Move your spaceship\n" +
                "• Spacebar - Shoot at enemies\n" +
                "• R - Restart (when game over)\n" +
                "• Esc - Return to Main Menu\n\n" +
                "Power-Ups:\n" +
                "• Red Square (PU) - Speed Boost for 45 seconds\n" +
                "• Gold Square (OHK) - One-Hit Kill for 30 seconds\n\n" +
                "Destroy all enemies before they reach you!\n" +
                "Good luck!",
                "How to Play",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}