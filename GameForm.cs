namespace SpaceRacer
{
    using System;
    using System.Drawing;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class GameForm : Form
    {
        private const int GameWidth = 800;
        private const int GameHeight = 600;
    private System.Windows.Forms.Timer gameTimer = null!;
    private Player player = null!;
        private List<Bullet> bullets = new List<Bullet>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Explosion> explosions = new List<Explosion>();
        private List<PowerUp> powerups = new List<PowerUp>();
        private HashSet<Keys> keysDown = new HashSet<Keys>();
        private Random rng = new Random();
    private WaveManager waveManager = null!;
        private int score = 0;
        private int lives = 3;
    private Button restartButton = null!;
    private Button menuButton = null!;
    private bool isGameOver = false;

        public GameForm()
        {
            InitializeComponents();
            InitializeGame();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(GameWidth, GameHeight);
            this.Text = "Space Racer";
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            gameTimer = new Timer();
            gameTimer.Interval = 16; // Approximately 60 FPS
            gameTimer.Tick += GameTimer_Tick;

            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
            this.DoubleBuffered = true;
            // allow the form to receive key events before child controls (so space/arrow keys always work)
            this.KeyPreview = true;

            // Game over UI (hidden until needed)
            restartButton = new Button
            {
                Text = "Restart (R)",
                Size = new Size(180, 48),
                Location = new Point((GameWidth / 2) - 200, GameHeight / 2 + 40),
                Visible = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 255),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            restartButton.Click += (s, e) => RestartGame();

            menuButton = new Button
            {
                Text = "Main Menu (Esc)",
                Size = new Size(180, 48),
                Location = new Point((GameWidth / 2) + 20, GameHeight / 2 + 40),
                Visible = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 255),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            menuButton.Click += (s, e) => 
            { 
                gameTimer.Stop();
                this.DialogResult = DialogResult.OK; // signal to return to menu
                this.Close(); 
            };

            foreach (Button button in new[] { restartButton, menuButton })
            {
                button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 150, 255);
                button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(0, 100, 255);
            }

            this.Controls.Add(restartButton);
            this.Controls.Add(menuButton);
        }

        private void InitializeGame()
        {
            player = new Player(new Point(GameWidth / 2 - 20, GameHeight - 80));
            bullets = new List<Bullet>();
            enemies = new List<Enemy>();
            explosions = new List<Explosion>();
            powerups = new List<PowerUp>();
            waveManager = new WaveManager(GameWidth, GameHeight);

            score = 0;
            lives = 3;

            gameTimer.Start();
            // ensure form has focus so it receives keyboard input
            this.Focus();
        }

        private void UpdateExplosions()
        {
            // run at approximately gameTimer interval (~16ms)
            float delta = gameTimer?.Interval ?? 16;
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(delta);
                if (!explosions[i].IsAlive()) explosions.RemoveAt(i);
            }
        }

        private void UpdatePowerUps()
        {
            for (int i = powerups.Count - 1; i >= 0; i--)
            {
                powerups[i].Update();
                if (!powerups[i].IsAlive(GameHeight)) powerups.RemoveAt(i);
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            HandleInput();
            UpdateBullets();
            UpdateEnemies();
            UpdatePowerUps();
            waveManager.Update(enemies, rng);
            UpdateExplosions();
            CheckCollisions();

            // Advance wave if no enemies remain and wave manager finished spawning
            if (enemies.Count == 0 && waveManager.IsWaveCleared)
            {
                waveManager.NextWave();
            }

            // detect game over and show UI once
            if (lives <= 0 && !isGameOver)
            {
                OnGameOver();
            }

            this.Invalidate();
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            keysDown.Add(e.KeyCode);
            // allow pressing escape to return to menu
            if (e.KeyCode == Keys.Escape)
            {
                gameTimer.Stop();
                this.Close();
            }
            // restart when game over
            if (e.KeyCode == Keys.R && isGameOver)
            {
                RestartGame();
            }
        }

        private void GameForm_KeyUp(object? sender, KeyEventArgs e)
        {
            keysDown.Remove(e.KeyCode);
        }

        private void HandleInput()
        {
            int dx = 0;
            int dy = 0;
            if (keysDown.Contains(Keys.Left) || keysDown.Contains(Keys.A)) dx -= 1;
            if (keysDown.Contains(Keys.Right) || keysDown.Contains(Keys.D)) dx += 1;
            if (keysDown.Contains(Keys.Up) || keysDown.Contains(Keys.W)) dy -= 1;
            if (keysDown.Contains(Keys.Down) || keysDown.Contains(Keys.S)) dy += 1;

            player.Move(dx, dy, GameWidth, GameHeight);

            if (keysDown.Contains(Keys.Space))
            {
                var b = player.TryFire();
                if (b != null) bullets.Add(b);
            }
        }

        private void UpdateBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update();
                if (!bullets[i].IsAlive(GameWidth, GameHeight)) bullets.RemoveAt(i);
            }
        }

        private void UpdateEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update();
                if (enemies[i].Position.Y > GameHeight + 50) // off screen bottom
                {
                    enemies.RemoveAt(i);
                    // possible penalty
                    lives = Math.Max(0, lives - 1);
                }
            }
        }

        private void CheckCollisions()
        {
            // Bullets vs Enemies
            for (int bi = bullets.Count - 1; bi >= 0; bi--)
            {
                var b = bullets[bi];
                bool removedBullet = false;
                for (int ei = enemies.Count - 1; ei >= 0; ei--)
                {
                    if (b.Bounds.IntersectsWith(enemies[ei].Bounds))
                    {
                        // create explosion at enemy center when hit
                        var ex = enemies[ei].Bounds.X + enemies[ei].Bounds.Width / 2;
                        var ey = enemies[ei].Bounds.Y + enemies[ei].Bounds.Height / 2;
                        explosions.Add(new Explosion(new System.Drawing.PointF(ex, ey), 400f, 34f));

                        // if one-hit-kill is active, instantly destroy
                        if (player.HasOneHitKill())
                        {
                            enemies[ei].Health = 0;
                        }
                        else
                        {
                            enemies[ei].Health -= b.Damage;
                        }

                        bullets.RemoveAt(bi);
                        removedBullet = true;
                        if (enemies[ei].Health <= 0)
                        {
                            score += enemies[ei].ScoreValue;
                            // create burst of small explosions when enemy is eliminated
                            CreateExplosionBurst(ex, ey);
                            // randomly spawn a power-up when enemy is destroyed
                            if (rng.Next(0, 100) < 25) // 25% chance
                            {
                                var puPos = new Point(enemies[ei].Bounds.X + enemies[ei].Bounds.Width / 2 - 16, enemies[ei].Bounds.Y);
                                // 80% speed boost, 20% one-hit-kill
                                var puType = rng.Next(0, 100) < 80 ? PowerUp.PowerUpType.SpeedBoost : PowerUp.PowerUpType.OneHitKill;
                                powerups.Add(new PowerUp(puPos, puType));
                            }
                            enemies.RemoveAt(ei);
                        }
                        break;
                    }
                }
                if (removedBullet) continue;
            }

            // Enemies vs Player
            foreach (var enemy in enemies)
            {
                if (enemy.Bounds.IntersectsWith(player.Bounds))
                {
                    // simple collision damage
                    lives = Math.Max(0, lives - 1);
                    enemy.Health = 0;
                }
            }

            // remove destroyed enemies
            enemies.RemoveAll(e => e.Health <= 0);

            // Player vs PowerUps
            for (int pi = powerups.Count - 1; pi >= 0; pi--)
            {
                if (powerups[pi].Bounds.IntersectsWith(player.Bounds))
                {
                    if (powerups[pi].Type == PowerUp.PowerUpType.SpeedBoost)
                    {
                        player.ApplySpeedBoost(45000); // 45 seconds
                    }
                    else if (powerups[pi].Type == PowerUp.PowerUpType.OneHitKill)
                    {
                        player.ApplyOneHitKill(30000); // 30 seconds
                    }
                    powerups.RemoveAt(pi);
                }
            }
        }

        private void CreateExplosionBurst(float centerX, float centerY)
        {
            // Create a burst of 4 small explosions radiating outward
            int numExplosions = 4;
            for (int i = 0; i < numExplosions; i++)
            {
                float angle = (float)(i * 2 * Math.PI / numExplosions);
                float offsetDist = 15f;
                float x = centerX + offsetDist * (float)Math.Cos(angle);
                float y = centerY + offsetDist * (float)Math.Sin(angle);
                explosions.Add(new Explosion(new System.Drawing.PointF(x, y), 300f, 20f));
            }
            // Add one large explosion at center
            explosions.Add(new Explosion(new System.Drawing.PointF(centerX, centerY), 350f, 25f));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // draw background with planets using wave manager
            waveManager.DrawBackground(g);

            // draw player
            player.Draw(g);

            // draw bullets
            foreach (var b in bullets) b.Draw(g);

            // draw enemies
            foreach (var enemy in enemies) enemy.Draw(g);

            // draw power-ups
            foreach (var pu in powerups) pu.Draw(g);

            // draw explosions (above enemies)
            foreach (var ex in explosions) ex.Draw(g);

            // HUD
            using (var font = new Font("Consolas", 12))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString($"Score: {score}", font, brush, 10, 10);
                g.DrawString($"Lives: {lives}", font, brush, 10, 30);
                g.DrawString($"Wave: {waveManager.CurrentWave}", font, brush, 10, 50);

                // show speed boost timer if active
                var boostMs = player.GetSpeedBoostRemainingMs();
                if (boostMs > 0)
                {
                    var boostSec = (boostMs + 999) / 1000; // round up
                    using (var boostBrush = new SolidBrush(Color.Lime))
                    {
                        g.DrawString($"SPEED BOOST: {boostSec}s", font, boostBrush, GameWidth - 220, 10);
                    }
                }

                // show one-hit-kill timer if active
                var ohkMs = player.GetOneHitKillRemainingMs();
                if (ohkMs > 0)
                {
                    var ohkSec = (ohkMs + 999) / 1000; // round up
                    using (var ohkBrush = new SolidBrush(Color.Gold))
                    {
                        g.DrawString($"ONE-HIT KILL: {ohkSec}s", font, ohkBrush, GameWidth - 240, 30);
                    }
                }
            }

            // if game over, we still draw overlay text; actual timer stop and UI visibility are handled
            if (lives <= 0)
            {
                using (var font = new Font("Arial", 32, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Yellow))
                {
                    var text = "GAME OVER";
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, (GameWidth - size.Width) / 2, (GameHeight - size.Height) / 2);
                }
                // draw restart instructions
                using (var font = new Font("Consolas", 14, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.White))
                {
                    var msg = "Press R to Restart or Esc to Exit";
                    var msize = g.MeasureString(msg, font);
                    g.DrawString(msg, font, brush, (GameWidth - msize.Width) / 2, (GameHeight - msize.Height) / 2 + 50);
                }
            }
        }

        private void OnGameOver()
        {
            isGameOver = true;
            try
            {
                gameTimer.Stop();
            }
            catch { }

            // show UI buttons
            if (restartButton != null) restartButton.Visible = true;
            if (menuButton != null) menuButton.Visible = true;
            restartButton?.BringToFront();
            menuButton?.BringToFront();
            restartButton?.Focus();
        }

        private void RestartGame()
        {
            // clear input to avoid immediate actions
            keysDown.Clear();
            // reset core state
            player = new Player(new Point(GameWidth / 2 - 20, GameHeight - 80));
            bullets = new List<Bullet>();
            enemies = new List<Enemy>();
            explosions = new List<Explosion>();
            powerups = new List<PowerUp>();
            waveManager = new WaveManager(GameWidth, GameHeight);
            score = 0;
            lives = 3;
            // hide UI
            isGameOver = false;
            if (restartButton != null) restartButton.Visible = false;
            if (menuButton != null) menuButton.Visible = false;
            gameTimer.Start();
        }
    }
}