namespace SpaceRacer
{
    using System.Drawing;

    public class Player
    {
        public Rectangle Bounds;
        private int baseSpeed = 6;
        private int currentSpeed = 6;
        private long lastFire = 0;
        private int fireCooldownMs = 100;
        private long speedBoostEndTick = 0;
        private long oneHitKillEndTick = 0;
        private Image? shipImage = null;

        public Player(Point start)
        {
            Bounds = new Rectangle(start.X, start.Y, 40, 60);
            LoadShipImage();
        }

        private void LoadShipImage()
        {
            try
            {
                string imagePath = "spaceship.png";
                if (System.IO.File.Exists(imagePath))
                {
                    shipImage = Image.FromFile(imagePath);
                }
            }
            catch
            {
                // if image fails to load, we'll fall back to drawing shapes in Draw()
                shipImage = null;
            }
        }

        public void ApplySpeedBoost(long durationMs = 45000)
        {
            speedBoostEndTick = System.Environment.TickCount + durationMs;
            currentSpeed = 12; // double speed
        }

        public void ApplyOneHitKill(long durationMs = 30000)
        {
            oneHitKillEndTick = System.Environment.TickCount + durationMs;
        }

        public long GetSpeedBoostRemainingMs()
        {
            var now = System.Environment.TickCount;
            if (now >= speedBoostEndTick) return 0;
            return speedBoostEndTick - now;
        }

        public long GetOneHitKillRemainingMs()
        {
            var now = System.Environment.TickCount;
            if (now >= oneHitKillEndTick) return 0;
            return oneHitKillEndTick - now;
        }

        public bool HasOneHitKill() => GetOneHitKillRemainingMs() > 0;

        private void UpdateSpeedBoost()
        {
            var now = System.Environment.TickCount;
            if (now >= speedBoostEndTick)
            {
                currentSpeed = baseSpeed;
            }
        }

        public Rectangle GetBounds() => Bounds;

        public void Move(int dx, int dy, int maxW, int maxH)
        {
            UpdateSpeedBoost();
            var nx = Bounds.X + dx * currentSpeed;
            var ny = Bounds.Y + dy * currentSpeed;
            nx = System.Math.Max(0, System.Math.Min(maxW - Bounds.Width, nx));
            ny = System.Math.Max(0, System.Math.Min(maxH - Bounds.Height, ny));
            Bounds = new Rectangle(nx, ny, Bounds.Width, Bounds.Height);
        }

        public Bullet? TryFire()
        {
            var now = System.Environment.TickCount;
            if (now - lastFire < fireCooldownMs) return null;
            lastFire = now;
            var bx = Bounds.X + Bounds.Width / 2 - 5;
            var by = Bounds.Y - 10;
            return new Bullet(new Rectangle(bx, by, 10, 16), -12, 1);
        }

        public void Draw(Graphics g)
        {
            if (shipImage != null)
            {
                // draw the PNG image scaled to the player bounds
                g.DrawImage(shipImage, Bounds);
            }
            else
            {
                // pixelated spaceship design
                int x = Bounds.X;
                int y = Bounds.Y;
                int w = Bounds.Width;
                int h = Bounds.Height;

                using (var brush = new SolidBrush(Color.Cyan))
                {
                    // main body - tapered fuselage
                    Point[] bodyPoints = new Point[]
                    {
                        new Point(x + w/2, y),           // top point
                        new Point(x + w - 6, y + h/3),   // upper right
                        new Point(x + w - 4, y + h),     // bottom right
                        new Point(x + 4, y + h),         // bottom left
                        new Point(x + 6, y + h/3)        // upper left
                    };
                    g.FillPolygon(brush, bodyPoints);
                }

                // wings (side thrusters)
                using (var wingBrush = new SolidBrush(Color.LimeGreen))
                {
                    // left wing
                    g.FillRectangle(wingBrush, x - 2, y + h / 2 - 3, 4, 8);
                    // right wing
                    g.FillRectangle(wingBrush, x + w - 2, y + h / 2 - 3, 4, 8);
                }

                // cockpit window
                using (var cockpitBrush = new SolidBrush(Color.Yellow))
                {
                    g.FillRectangle(cockpitBrush, x + w / 2 - 4, y + 6, 8, 8);
                }

                // thruster glow at bottom
                using (var thrusterBrush = new SolidBrush(Color.OrangeRed))
                {
                    g.FillRectangle(thrusterBrush, x + w / 2 - 3, y + h - 4, 6, 4);
                }
            }
        }
    }

    public class Bullet
    {
        public Rectangle Bounds;
        private int dy;
        public int Damage { get; private set; }

        public Bullet(Rectangle rect, int dy, int damage)
        {
            Bounds = rect;
            this.dy = dy;
            Damage = damage;
        }

        public void Update()
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y + dy, Bounds.Width, Bounds.Height);
        }

        public bool IsAlive(int w, int h)
        {
            return Bounds.Bottom >= 0 && Bounds.Top <= h && Bounds.Left >= 0 && Bounds.Right <= w + 100;
        }

        public Rectangle GetBounds() => Bounds;

        public void Draw(Graphics g)
        {
            using (var brush = new SolidBrush(Color.Yellow))
            {
                g.FillRectangle(brush, Bounds);
            }
        }
    }

    public class Enemy
    {
        public Rectangle Bounds;
        public int Health;
        private int speed;
        public int ScoreValue { get; private set; }

        public Enemy(Rectangle rect, int health, int speed, int score)
        {
            Bounds = rect;
            Health = health;
            this.speed = speed;
            ScoreValue = score;
        }

        public Point Position => new Point(Bounds.X, Bounds.Y);

        public void Update()
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y + speed, Bounds.Width, Bounds.Height);
        }

        public Rectangle GetBounds() => Bounds;

        public void Draw(Graphics g)
        {
            Color c = Health >= 3 ? Color.Red : (Health == 2 ? Color.Orange : Color.Pink);
            
            int x = Bounds.X;
            int y = Bounds.Y;
            int w = Bounds.Width;
            int h = Bounds.Height;

            // Pixelated alien design
            using (var brush = new SolidBrush(c))
            {
                // large head (top half)
                g.FillEllipse(brush, x + 4, y, w - 8, h / 2 + 4);
                
                // body (bottom half, narrower)
                g.FillRectangle(brush, x + 8, y + h / 2 - 2, w - 16, h / 2 + 2);
            }

            // left eye
            using (var eyeBrush = new SolidBrush(Color.Black))
            {
                g.FillEllipse(eyeBrush, x + 8, y + 6, 5, 6);
            }

            // right eye
            using (var eyeBrush = new SolidBrush(Color.Black))
            {
                g.FillEllipse(eyeBrush, x + w - 13, y + 6, 5, 6);
            }

            // left arm
            using (var armBrush = new SolidBrush(c))
            {
                g.FillRectangle(armBrush, x - 2, y + h / 3, 4, h / 3);
            }

            // right arm
            using (var armBrush = new SolidBrush(c))
            {
                g.FillRectangle(armBrush, x + w - 2, y + h / 3, 4, h / 3);
            }

            // simple health bar
            using (var pen = new Pen(Color.Black, 2))
            {
                var healthBarWidth = Bounds.Width * Health / 3;
                g.DrawRectangle(pen, Bounds.X, Bounds.Y - 6, Bounds.Width, 4);
                using (var hb = new SolidBrush(Color.Lime))
                {
                    g.FillRectangle(hb, Bounds.X + 1, Bounds.Y - 5, System.Math.Max(0, healthBarWidth - 2), 2);
                }
            }
        }
    }

    public class Explosion
    {
        public PointF Position;
        public float AgeMs;
        public float LifetimeMs;
        public float MaxRadius;

        public Explosion(PointF pos, float lifetimeMs = 400f, float maxRadius = 34f)
        {
            Position = pos;
            AgeMs = 0f;
            LifetimeMs = lifetimeMs;
            MaxRadius = maxRadius;
        }

        public void Update(float deltaMs)
        {
            AgeMs += deltaMs;
        }

        public bool IsAlive() => AgeMs < LifetimeMs;

        public void Draw(Graphics g)
        {
            float t = Math.Max(0f, Math.Min(1f, AgeMs / LifetimeMs));
            float r = MaxRadius * t;
            int alpha = (int)(255 * (1f - t));
            if (alpha < 0) alpha = 0;
            using (var brush = new SolidBrush(Color.FromArgb(alpha, 255, 200, 50)))
            {
                g.FillEllipse(brush, Position.X - r, Position.Y - r, r * 2, r * 2);
            }
        }
    }

    public class PowerUp
    {
        public Rectangle Bounds;
        private int speed = 2;
        public enum PowerUpType { SpeedBoost, OneHitKill }
        public PowerUpType Type { get; private set; }

        public PowerUp(Point pos, PowerUpType type = PowerUpType.SpeedBoost)
        {
            Bounds = new Rectangle(pos.X, pos.Y, 32, 32);
            Type = type;
        }

        public void Update()
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y + speed, Bounds.Width, Bounds.Height);
        }

        public bool IsAlive(int screenHeight)
        {
            return Bounds.Top < screenHeight + 50;
        }

        public Rectangle GetBounds() => Bounds;

        public void Draw(Graphics g)
        {
            if (Type == PowerUpType.SpeedBoost)
            {
                // bright red square
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.FillRectangle(brush, Bounds);
                }
                // "PU" text
                using (var font = new Font("Arial", 14, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.White))
                {
                    var text = "Power";
                    var size = g.MeasureString(text, font);
                    var x = Bounds.X + (Bounds.Width - size.Width) / 2;
                    var y = Bounds.Y + (Bounds.Height - size.Height) / 2;
                    g.DrawString(text, font, textBrush, x, y);
                }
            }
            else if (Type == PowerUpType.OneHitKill)
            {
                // bright gold square
                using (var brush = new SolidBrush(Color.Gold))
                {
                    g.FillRectangle(brush, Bounds);
                }
                // "OHK" text
                using (var font = new Font("Arial", 12, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.White))
                {
                    var text = "One-Hit";
                    var size = g.MeasureString(text, font);
                    var x = Bounds.X + (Bounds.Width - size.Width) / 1;
                    var y = Bounds.Y + (Bounds.Height - size.Height) / 1;
                    g.DrawString(text, font, textBrush, x, y);
                }
            }
        }
    }
}
