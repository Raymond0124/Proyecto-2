using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Proyecto
{
    public partial class MainForm : Form
    {
        private Timer gameTimer = new Timer();
        private List<Platform> platforms = new();
        private List<Token> tokens = new();
        private List<Player> players = new();
        private Random rnd = new Random();
        private int gameTime = 60;






        // Nueva variable para animar la construcci�n del �rbol
        private bool treeUpdated = false;

        // Panel para mostrar �rboles B
        private Panel treePanel;

        // Colores para los jugadores
        private Color[] playerColors = {
            Color.RoyalBlue,
            Color.Crimson,
            Color.ForestGreen,
            Color.DarkOrange
        };

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // Configurar el panel de �rboles
            ConfigureTreePanel();

            // Inicializar jugadores
            players.Add(new Player(100, 300, ControlType.Keyboard, 0, Keys.A, Keys.D, Keys.W));       // Jugador 1 (WASD)
            players.Add(new Player(700, 300, ControlType.Keyboard, 0, Keys.Left, Keys.Right, Keys.Up)); // Jugador 2 (Flechas)
            players.Add(new Player(400, 100, ControlType.Gamepad, 0)); // Jugador 3 (control 1)
            players.Add(new Player(500, 100, ControlType.Gamepad, 1)); // Jugador 4 (control 2)

            // Inicializa los BTree de cada jugador
            foreach (var player in players)
            {
                player.Tree = new BTree(3); // Grado 3 para empezar
            }

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

            Timer tokenTimer = new Timer { Interval = 5000 }; // cada 5 segundos
            tokenTimer.Tick += (s, e) => GenerateTokens();
            tokenTimer.Start();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }

        private void ConfigureTreePanel()
        {
            // Crear un panel para mostrar los �rboles B
            treePanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(treePanel);
            treePanel.Paint += TreePanel_Paint;
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Dibujar t�tulo del panel
            g.DrawString("�rboles B en Tiempo Real", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, 10, 10);

            // Dibujar l�nea separadora
            g.DrawLine(Pens.Gray, 0, 40, treePanel.Width, 40);

            // �rea para dibujar �rboles B de ejemplo


            // Dibujar �rboles de cada jugador
            int y = 250;
            for (int i = 0; i < players.Count; i++)
            {
                // S�lo mostrar si el jugador tiene tokens recolectados
                if (players[i].Score > 0)
                {
                    string playerName = $"P{i + 1}: {players[i].Score} pts";
                    g.DrawString(playerName, new Font("Arial", 10, FontStyle.Bold),
                                new SolidBrush(playerColors[i % playerColors.Length]), 10, y - 200);

                    // Dibujar el �rbol B del jugador
                    DrawCompactBTree(g, players[i].Tree, 20, y, playerColors[i % playerColors.Length]);

                    y += 100; // Espacio vertical entre �rboles de jugadores
                }
            }
        }




        private void DrawExampleNode(Graphics g, int x, int y, string[] keys, Pen outline = null, Brush fill = null)
        {
            if (outline == null) outline = Pens.Black;
            if (fill == null) fill = Brushes.White;

            int width = keys.Length * 15;
            g.FillEllipse(fill, x - width / 2, y - 10, width, 20);
            g.DrawEllipse(outline, x - width / 2, y - 10, width, 20);

            // Dibujar las claves
            for (int i = 0; i < keys.Length; i++)
            {
                int keyX = x - width / 2 + i * 15 + 7;
                g.DrawString(keys[i], new Font("Arial", 8), Brushes.Black, keyX - 4, y - 7);
            }
        }

        private void DrawCompactBTree(Graphics g, BTree tree, int x, int y, Color color)
        {
            if (tree.Root != null)
            {
                // Usar colores personalizados por jugador
                Brush nodeBrush = new SolidBrush(Color.FromArgb(180, color));
                Pen nodePen = new Pen(Color.FromArgb(240, color), 2);

                // Dibujar el �rbol en un tama�o m�s compacto para el panel lateral
                tree.Draw(g, x + 180, y - 180);

                // Indicar actualizaci�n visual si el �rbol cambi�
                if (treeUpdated)
                {
                    g.DrawString("�Actualizado!", new Font("Arial", 8, FontStyle.Bold),
                                Brushes.Green, x + 150, y - 200);
                    treeUpdated = false;
                }
            }
            else
            {
                g.DrawString("�rbol vac�o", new Font("Arial", 8), Brushes.Gray, x, y);
            }
        }

        private void GeneratePlatforms()
        {
            platforms.Clear();

            // Plataforma principal centrada
            int mainPlatformWidth = 500;
            int mainPlatformX = (this.ClientSize.Width - treePanel.Width - mainPlatformWidth) / 2;
            platforms.Add(new Platform(mainPlatformX, this.ClientSize.Height - 50, mainPlatformWidth, 20));



            int maxWidth = this.ClientSize.Width - treePanel.Width;
            int maxHeight = this.ClientSize.Height;
            // Define zonas de aparici�n para las plataformas aleatorias (en porcentaje del �rea jugable)
            var zones = new (float minX, float maxX, float minY, float maxY)[]
            {
        (0.0f, 0.3f, 0.72f, 0.75f),
        (0.4f, 0.6f, 0.75f, 0.75f),
        (0.7f, 1f, 0.73f, 0.75f),
        (0.1f, 0.3f, 0.45f, 0.45f),
        (0.4f, 0.6f, 0.45f, 0.48f),
        (0.7f, 0.9f, 0.45f, 0.47f),
        (0.4f, 0.7f, 0.6f, 0.6f),

            };

            // Generar las plataformas en sus respectivas zonas
            foreach (var zone in zones)
            {
                int x = rnd.Next((int)(zone.minX * maxWidth), (int)(zone.maxX * maxWidth) - 150);
                int y = rnd.Next((int)(zone.minY * maxHeight), (int)(zone.maxY * maxHeight));
                platforms.Add(new Platform(x, y, 150, 20));
            }
        }


        private void GenerateTokens()
        {
            tokens.Clear();
            for (int i = 0; i < 5; i++) // Aumentamos a 5 tokens
            {
                int x = rnd.Next(100, this.ClientSize.Width - treePanel.Width - 100);
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

            // Empuj�n entre jugadores si se tocan
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

            bool anyTokenCollected = false;

            foreach (var token in tokens)
            {
                foreach (var player in players)
                {
                    if (!token.Collected && token.Bounds.IntersectsWith(player.Bounds))
                    {
                        token.Collected = true;
                        player.Score++;

                        // Insertar el token en el �rbol del jugador
                        player.Tree.Insert(token.Value);

                        // Marcar que se actualiz� un �rbol
                        treeUpdated = true;
                        anyTokenCollected = true;

                        // Actualizar inmediatamente el panel de �rboles
                        treePanel.Invalidate();

                        break;
                    }
                }
            }


            // Limpiar tokens colectados
            tokens.RemoveAll(t => t.Collected);

            // Si se recogi� alg�n token, generar uno nuevo despu�s de un peque�o retraso
            if (anyTokenCollected && tokens.Count < 5)
            {
                Timer newTokenTimer = new Timer { Interval = 1000 };
                newTokenTimer.Tick += (s, args) =>
                {
                    int x = rnd.Next(100, this.ClientSize.Width - treePanel.Width - 100);
                    int y = rnd.Next(100, this.ClientSize.Height - 100);
                    int value = rnd.Next(1, 100);
                    tokens.Add(new Token(x, y, value));
                    newTokenTimer.Stop();
                    newTokenTimer.Dispose();
                };
                newTokenTimer.Start();
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

            // Dibujar fondo
            g.Clear(Color.LightSkyBlue);

            // Texto en la parte superior
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            g.DrawString("Build B-Tree!", titleFont, Brushes.Black, (this.ClientSize.Width - treePanel.Width) / 2 - 80, 20);

            // Dibujar plataformas
            foreach (var platform in platforms)
                platform.Draw(g);

            // Dibujar tokens
            foreach (var token in tokens)
                token.Draw(g);

            // Dibujar jugadores con colores espec�ficos
            for (int i = 0; i < players.Count; i++)
            {
                Pen playerPen = new Pen(playerColors[i % playerColors.Length], 2);
                players[i].Draw(g, playerPen);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1150, 600);  // Ventana m�s ancha para el panel lateral
            this.Name = "MainForm";
            this.Text = "Super Smash Trees";
            this.ResumeLayout(false);
        }
    }
}