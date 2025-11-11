namespace SpaceRacer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public class WaveManager
    {
        private int width;
        private int height;
        public int CurrentWave { get; private set; } = 1;
        private int spawnTarget;
        private int spawnedCount = 0;
        private int spawnIntervalMs = 900;
        private int lastSpawnTick = Environment.TickCount;
        private bool spawnComplete = false;

        private Color[] backgrounds = new Color[]
        {
            Color.Black,
            Color.DarkBlue,
            Color.DarkSlateGray,
            Color.MidnightBlue,
            Color.DarkRed,
            Color.Purple
        };

        private Color[] planetColors = new Color[]
        {
            Color.OrangeRed,      // Mars-like
            Color.RoyalBlue,      // Neptune-like
            Color.LimeGreen,      // Alien world
            Color.Yellow,         // Jupiter-like
            Color.SaddleBrown,    // Desert world
            Color.Magenta         // Exotic world
        };

        public WaveManager(int w, int h)
        {
            width = w;
            height = h;
            SetupWave();
        }

        public bool IsWaveCleared => spawnComplete;

        public Color CurrentBackground => backgrounds[(CurrentWave - 1) % backgrounds.Length];

        public Color CurrentPlanetColor => planetColors[(CurrentWave - 1) % planetColors.Length];

        public void DrawBackground(Graphics g)
        {
            // draw main background color
            using (var brush = new SolidBrush(CurrentBackground))
            {
                g.FillRectangle(brush, 0, 0, width, height);
            }

            // draw planet in background (large, faded)
            Color planetColor = CurrentPlanetColor;
            int planetX = width - 150;
            int planetY = height - 200;
            int planetRadius = 100;

            // main planet circle (semi-transparent)
            using (var brush = new SolidBrush(Color.FromArgb(80, planetColor.R, planetColor.G, planetColor.B)))
            {
                g.FillEllipse(brush, planetX - planetRadius, planetY - planetRadius, planetRadius * 2, planetRadius * 2);
            }

            // planet outline
            using (var pen = new Pen(Color.FromArgb(120, planetColor), 2))
            {
                g.DrawEllipse(pen, planetX - planetRadius, planetY - planetRadius, planetRadius * 2, planetRadius * 2);
            }

            // add some craters/details based on wave
            Random rng = new Random(CurrentWave); // deterministic based on wave
            for (int i = 0; i < 5; i++)
            {
                int cx = planetX + rng.Next(-planetRadius + 20, planetRadius - 20);
                int cy = planetY + rng.Next(-planetRadius + 20, planetRadius - 20);
                int cr = rng.Next(8, 20);
                using (var craterBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                {
                    g.FillEllipse(craterBrush, cx - cr, cy - cr, cr * 2, cr * 2);
                }
            }

            // add stars
            using (var starBrush = new SolidBrush(Color.White))
            {
                Random starRng = new Random(CurrentWave * 7); // different seed for stars
                for (int i = 0; i < 12; i++)
                {
                    int sx = starRng.Next(0, width);
                    int sy = starRng.Next(0, height / 2); // stars only in upper half
                    int size = starRng.Next(1, 3);
                    g.FillRectangle(starBrush, sx, sy, size, size);
                }
            }
        }

        private void SetupWave()
        {
            spawnTarget = 4 + CurrentWave * 3;
            spawnedCount = 0;
            spawnIntervalMs = Math.Max(220, 900 - (CurrentWave - 1) * 80);
            lastSpawnTick = Environment.TickCount;
            spawnComplete = false;
        }

        public void Update(List<Enemy> enemies, Random rng)
        {
            // If we're still spawning this wave, spawn according to interval
            if (!spawnComplete && spawnedCount < spawnTarget)
            {
                var now = Environment.TickCount;
                if (now - lastSpawnTick >= spawnIntervalMs)
                {
                    lastSpawnTick = now;
                    // spawn a burst of 1-2 enemies
                    int burst = rng.Next(1, 3);
                    for (int i = 0; i < burst && spawnedCount < spawnTarget; i++)
                    {
                        int ex = rng.Next(20, width - 60);
                        int ey = -rng.Next(40, 120);
                        int health = 1 + (CurrentWave / 3) + rng.Next(0, 2);
                        int speed = 1 + (CurrentWave / 4) + rng.Next(0, 2);
                        int score = 10 + CurrentWave * 2;
                        enemies.Add(new Enemy(new Rectangle(ex, ey, 40, 40), health, speed, score));
                        spawnedCount++;
                    }
                }
            }

            if (spawnedCount >= spawnTarget)
            {
                spawnComplete = true;
            }
        }

        public void NextWave()
        {
            CurrentWave++;
            SetupWave();
        }
    }
}
