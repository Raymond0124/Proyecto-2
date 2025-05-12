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
        List<Player> players = new();

        Random rnd = new Random();
        int gameTime = 60;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            players.Add(new Player(100, 300, ControlType.Keyboard, 0, Keys.A, Keys.D, Keys.W));       // Jugador 1 (WASD)
            players.Add(new Player(700, 300, ControlType.Keyboard, 0, Keys.Left, Keys.Right, Keys.Up)); // Jugador 2 (Flechas)
            players.Add(new Player(400, 100, ControlType.Gamepad, 0)); // Jugador 3 (control 1)
            players.Add(new Player(500, 100, ControlType.Gamepad, 1)); // Jugador 4 (control 2)


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
                    MessageBox.Show($"Juego terminado.\n" +
                $"P1: {players[0].Score} puntos\n" +
                $"P2: {players[1].Score} puntos\n" +
                $"P3: {players[2].Score} puntos\n" +
                $"P4: {players[3].Score} puntos");

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

            
            foreach (var player in players)
                player.Update(platforms);

            foreach (var player in players)
            {
                if (player.Y > this.ClientSize.Height)
                    RespawnPlayer(player);
            }


            // Empujón entre jugadores si se tocan
            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    var a = players[i];
                    var b = players[j];
                    if (a.Bounds.IntersectsWith(b.Bounds))
                    {
                        Rectangle inter = Rectangle.Intersect(a.Bounds, b.Bounds);
                        if (inter.Width > 0)
                        {
                            if (a.X < b.X)
                            {
                                a.X -= inter.Width / 2;
                                b.X += inter.Width / 2;
                            }
                            else
                            {
                                a.X += inter.Width / 2;
                                b.X -= inter.Width / 2;
                            }
                        }
                    }
                }
            }

           
            foreach (var token in tokens)
            {
                foreach (var player in players)
                {
                    if (!token.Collected && token.Bounds.IntersectsWith(player.Bounds))
                    {
                        token.Collected = true;
                        player.Score++;
                        break; // solo 1 jugador puede recogerlo
                    }
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
            foreach (var player in players)
                player.KeyDown(e.KeyCode);
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            foreach (var player in players)
                player.KeyUp(e.KeyCode);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var platform in platforms)
                platform.Draw(g);

            foreach (var token in tokens)
                token.Draw(g);

            Pen[] pens = { Pens.Blue, Pens.Red, Pens.Green, Pens.Orange };
            for (int i = 0; i < players.Count; i++)
                players[i].Draw(g, pens[i % pens.Length]);

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
