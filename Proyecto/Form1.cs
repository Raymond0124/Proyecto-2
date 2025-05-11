using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Proyecto
{
    public partial class MainForm : Form
    {
        Timer gameTimer = new Timer();
        List<Platform> platforms = new();
        List<Token> tokens = new();
        Player player1, player2;
        Random rnd = new Random();
        int gameTime = 60;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            player1 = new Player(100, 300, Keys.A, Keys.D, Keys.W);
            player2 = new Player(700, 300, Keys.Left, Keys.Right, Keys.Up);

            GeneratePlatforms();
            GenerateTokens();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            Timer countdown = new Timer { Interval = 1000 };
            countdown.Tick += (s, e) =>
            {
                gameTime--;
                this.Text = $"Super Smash Trees - Tiempo restante: {gameTime}s";
                if (gameTime <= 0)
                {
                    countdown.Stop();
                    gameTimer.Stop();
                    MessageBox.Show($"Juego terminado.\nP1: {player1.Score} puntos\nP2: {player2.Score} puntos");
                }
            };
            countdown.Start();

            Timer tokenTimer = new Timer { Interval = 10000 }; // cada 10 segundos
            tokenTimer.Tick += (s, e) => GenerateTokens();
            tokenTimer.Start();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }

        private void GeneratePlatforms()
        {
            platforms.Clear();

            // Plataforma principal centrada
            int mainPlatformWidth = 500;
            int mainPlatformX = (this.ClientSize.Width - mainPlatformWidth) / 2;
            platforms.Add(new Platform(mainPlatformX, this.ClientSize.Height - 50, mainPlatformWidth, 20));

            // Plataformas aleatorias
            for (int i = 0; i < 5; i++)
            {
                int x = rnd.Next(50, this.ClientSize.Width - 150);
                int y = 100 + i * 80;
                platforms.Add(new Platform(x, y, 150, 20));
            }
        }

        private void GenerateTokens()
        {
            tokens.Clear();
            for (int i = 0; i < 3; i++)
            {
                int x = rnd.Next(100, this.ClientSize.Width - 100);
                int y = rnd.Next(100, this.ClientSize.Height - 100);
                int value = rnd.Next(1, 100);
                tokens.Add(new Token(x, y, value));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            player1.Update(platforms);
            player2.Update(platforms);

            // Reaparición si algún jugador cae fuera de la pantalla
            if (player1.Y > this.ClientSize.Height)
            {
                RespawnPlayer(player1);
            }
            if (player2.Y > this.ClientSize.Height)
            {
                RespawnPlayer(player2);
            }

            // Empujón entre jugadores si se tocan
            if (player1.Bounds.IntersectsWith(player2.Bounds))
            {
                Rectangle intersection = Rectangle.Intersect(player1.Bounds, player2.Bounds);
                if (intersection.Width > 0)
                {
                    if (player1.X < player2.X)
                    {
                        player1.X -= intersection.Width / 2;
                        player2.X += intersection.Width / 2;
                    }
                    else
                    {
                        player1.X += intersection.Width / 2;
                        player2.X -= intersection.Width / 2;
                    }
                }
            }

            // Recolección de tokens
            foreach (var token in tokens)
            {
                if (!token.Collected && token.Bounds.IntersectsWith(player1.Bounds))
                {
                    token.Collected = true;
                    player1.Score++;
                }
                if (!token.Collected && token.Bounds.IntersectsWith(player2.Bounds))
                {
                    token.Collected = true;
                    player2.Score++;
                }
            }

            Invalidate();
        }

        private void RespawnPlayer(Player player)
        {
            Platform p = platforms[rnd.Next(platforms.Count)];
            player.X = p.X + (p.Width - player.Width) / 2;
            player.Y = p.Y - player.Height;
            player.SpeedX = 0;
            player.SpeedY = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            player1.KeyDown(e.KeyCode);
            player2.KeyDown(e.KeyCode);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            player1.KeyUp(e.KeyCode);
            player2.KeyUp(e.KeyCode);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var platform in platforms)
                platform.Draw(g);

            foreach (var token in tokens)
                token.Draw(g);

            player1.Draw(g, Pens.Blue);
            player2.Draw(g, Pens.Red);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(900, 600);
            this.Name = "MainForm";
            this.Text = "Super Smash Trees";
            this.ResumeLayout(false);
        }
    }
}
