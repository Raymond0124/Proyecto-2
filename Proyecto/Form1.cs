using Proyecto.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private const int MaxAVLNodes = 7;
        private const int MaxBTreeNodes = 7;
        private const int MaxBSTNodes = 7;

        private ChallengeManager challengeManager = new ChallengeManager();
        private bool treeUpdated = false;
        private Panel treePanel;

        private Color[] playerColors = {
            Color.RoyalBlue,
            Color.Crimson,
            Color.ForestGreen,
            Color.DarkOrange
        };

        private Dictionary<Player, Player> lastPlayerContact = new Dictionary<Player, Player>();

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            ConfigureTreePanel();

            // Inicializar jugadores con teclas de poderes
            players.Add(new Player(100, 300, ControlType.Keyboard, 0, Keys.A, Keys.D, Keys.W, Keys.Q));
            players.Add(new Player(700, 300, ControlType.Keyboard, 0, Keys.Left, Keys.Right, Keys.Up, Keys.RShiftKey));
            players.Add(new Player(400, 100, ControlType.Gamepad, 0));
            players.Add(new Player(500, 100, ControlType.Gamepad, 1));

            foreach (var player in players)
            {
                lastPlayerContact[player] = null;
                // Dar poder inicial aleatorio a cada jugador
                player.AddPower(new Power(PowerType.Shield));
            }

            foreach (var player in players)
            {
                player.Tree = new BTree(3);
            }

            GeneratePlatforms();
            GenerateTokens();

            gameTimer.Interval = 20;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            Timer countdown = new Timer { Interval = 10000 };
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

                    var winner = players.OrderByDescending(p => p.Score).First();
                    int maxScore = winner.Score;
                    if (players.Count(p => p.Score == maxScore) > 1)
                        MessageBox.Show("¡Empate!");
                    else
                        MessageBox.Show($"¡Ganador: P{players.IndexOf(winner) + 1} con {maxScore} puntos!");
                }
            };
            countdown.Start();

            Timer tokenTimer = new Timer { Interval = 5000 };
            tokenTimer.Tick += (s, e) => GenerateTokens();
            tokenTimer.Start();

            // Timer para dar poderes aleatorios
            Timer powerTimer = new Timer { Interval = 10000 }; // Cada 10 segundos
            powerTimer.Tick += (s, e) =>
            {
                foreach (var player in players)
                {
                    if (player.Powers.Count < 3) // Máximo 3 poderes por jugador
                    {
                        var powerManager = new PowerManager();
                        var randomPower = powerManager.GetRandomPower();
                        player.AddPower(randomPower);
                    }
                }
            };
            powerTimer.Start();

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }

        private void ConfigureTreePanel()
        {
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

            g.DrawString("Árboles B en Tiempo Real", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, 10, 10);
            g.DrawLine(Pens.Gray, 0, 40, treePanel.Width, 40);

            int y = 250;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Score > 0)
                {
                    string playerName = $"P{i + 1}: {players[i].Score} pts";
                    g.DrawString(playerName, new Font("Arial", 10, FontStyle.Bold),
                                new SolidBrush(playerColors[i % playerColors.Length]), 10, y - 200);

                    if (players[i].CurrentChallenges != null && players[i].CurrentChallenges.Count > 0)
                    {
                        string currentChallenge = players[i].CurrentChallenges[0].Description;
                        g.DrawString("Reto: " + currentChallenge, new Font("Arial", 8, FontStyle.Italic),
                                     Brushes.Black, 10, y - 185);
                    }

                    DrawCompactTree(g, players[i], 20, y, playerColors[i % playerColors.Length]);
                    y += 135;
                }
            }
        }

        private void DrawCompactTree(Graphics g, Player player, int x, int y, Color color)
        {
            if (player.IsUsingAVL && player.AVLTree != null)
            {
                player.AVLTree.Draw(g, x + 180, y - 180);
            }
            else if (player.IsUsingBST && player.BSTree?.Root != null)
            {
                player.BSTree.Draw(g, x + 180, y - 180);
            }
            else if (!player.IsUsingBST && player.Tree?.Root != null)
            {
                Brush nodeBrush = new SolidBrush(Color.FromArgb(180, color));
                Pen nodePen = new Pen(Color.FromArgb(240, color), 2);
                player.Tree.Draw(g, x + 180, y - 180);

                if (treeUpdated)
                {
                    g.DrawString("¡Actualizado!", new Font("Arial", 8, FontStyle.Bold),
                                Brushes.Green, x + 150, y - 200);
                    treeUpdated = false;
                }
            }
            else
            {
                g.DrawString("Árbol vacío", new Font("Arial", 8), Brushes.Gray, x, y);
            }
        }

        private void GeneratePlatforms()
        {
            platforms.Clear();

            int mainPlatformWidth = 500;
            int mainPlatformX = (this.ClientSize.Width - treePanel.Width - mainPlatformWidth) / 2;
            platforms.Add(new Platform(mainPlatformX, this.ClientSize.Height - 50, mainPlatformWidth, 20));

            int maxWidth = this.ClientSize.Width - treePanel.Width;
            int maxHeight = this.ClientSize.Height;

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
            for (int i = 0; i < 5; i++)
            {
                int x = rnd.Next(100, this.ClientSize.Width - treePanel.Width - 100);
                int y = rnd.Next(100, this.ClientSize.Height - 100);
                int value = rnd.Next(1, 100);
                tokens.Add(new Token(x, y, value));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Actualizar jugadores pasando la lista completa para poderes
            foreach (var player in players)
                player.Update(platforms, players);

            foreach (var player in players)
            {
                if (player.Y > this.ClientSize.Height)
                {
                    Player pusher = lastPlayerContact[player];
                    if (pusher != null)
                    {
                        pusher.Score += 2;

                        int pushValue = rnd.Next(1, 100);
                        if (!pusher.IsUsingBST)
                        {
                            pusher.InsertKey(pushValue);

                            if (pusher.Tree.CountNodes() > MaxBTreeNodes)
                            {
                                pusher.Tree = null;
                                pusher.BSTree = new BSTree();
                                pusher.IsUsingBST = true;
                                treeUpdated = true;
                            }
                        }
                        else
                        {
                            pusher.BSTree.Insert(pushValue);

                            if (pusher.BSTree.CountNodes() > MaxBSTNodes && !pusher.IsUsingAVL)
                            {
                                var bstValues = pusher.BSTree.InOrderTraversal();
                                pusher.AVLTree = new AVLTree();
                                foreach (var v in bstValues)
                                    pusher.AVLTree.Insert(v);

                                pusher.BSTree = null;
                                pusher.IsUsingAVL = true;
                            }
                        }

                        treePanel.Invalidate();
                    }

                    lastPlayerContact[player] = null;
                    RespawnPlayer(player);
                }
            }

            // Empujón entre jugadores - ahora considerando Shield
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
                            bool aHasShield = a.HasShieldActive();
                            bool bHasShield = b.HasShieldActive();

                            if (!aHasShield && !bHasShield)
                            {
                                if (a.SpeedX != 0)
                                    lastPlayerContact[b] = a;

                                if (b.SpeedX != 0)
                                    lastPlayerContact[a] = b;

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
                            else
                            {
                                // Si alguno tiene escudo, solo separar sin registrar contacto
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
            }

            bool anyTokenCollected = false;

            foreach (var token in tokens)
            {
                foreach (var player in players)
                {
                    if (player.CurrentChallenges == null || player.CurrentChallenges.Count == 0)
                    {
                        player.CurrentChallenges = challengeManager.GetChallengesFor("BTree");
                    }

                    if (!token.Collected && token.Bounds.IntersectsWith(player.Bounds))
                    {
                        token.Collected = true;
                        player.Score++;
                        anyTokenCollected = true;

                        if (!player.IsUsingBST)
                        {
                            player.InsertKey(token.Value);

                            if (player.Tree.CountNodes() > MaxBTreeNodes)
                            {
                                bool anyChallengeCompleted = player.CurrentChallenges.Any(c => c.Condition(player));

                                if (anyChallengeCompleted)
                                {
                                    player.BSTree = new BSTree();
                                    player.Tree = null;
                                    player.IsUsingBST = true;
                                    treeUpdated = true;
                                    treePanel.Invalidate();

                                    player.CurrentChallenges = challengeManager.GetChallengesFor("BST");
                                }
                            }
                        }
                        else if (!player.IsUsingAVL)
                        {
                            if (player.BSTree != null)
                            {
                                player.BSTree.Insert(token.Value);

                                if (player.BSTree.CountNodes() > MaxBSTNodes && !player.IsUsingAVL)
                                {
                                    bool anyChallengeCompleted = player.CurrentChallenges.Any(c => c.Condition(player));

                                    if (anyChallengeCompleted)
                                    {
                                        player.AVLTree = new AVLTree();
                                        player.BSTree = null;
                                        player.IsUsingAVL = true;
                                        treeUpdated = true;
                                        treePanel.Invalidate();

                                        player.CurrentChallenges = challengeManager.GetChallengesFor("AVL");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (player.AVLTree != null)
                            {
                                player.AVLTree.Insert(token.Value);

                                if (player.AVLTree.CountNodes() > MaxAVLNodes)
                                {
                                    bool anyChallengeCompleted = player.CurrentChallenges.Any(c => c.Condition(player));

                                    if (anyChallengeCompleted)
                                    {
                                        player.Tree = new BTree(3);
                                        player.AVLTree = null;
                                        player.IsUsingBST = false;
                                        player.IsUsingAVL = false;
                                        treeUpdated = true;
                                        treePanel.Invalidate();

                                        player.CurrentChallenges = challengeManager.GetChallengesFor("BTree");
                                    }
                                }
                            }
                        }

                        treePanel.Invalidate();
                        break;
                    }
                }
            }

            tokens.RemoveAll(t => t.Collected);

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
                player.KeyDown(e.KeyCode, players); // Pasar lista de jugadores para poderes
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            foreach (var player in players)
                player.KeyUp(e.KeyCode);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.LightSkyBlue);

            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            g.DrawString("Build B-Tree!", titleFont, Brushes.Black, (this.ClientSize.Width - treePanel.Width) / 2 - 80, 20);

            foreach (var platform in platforms)
                platform.Draw(g);

            foreach (var token in tokens)
                token.Draw(g);

            for (int i = 0; i < players.Count; i++)
            {
                Pen playerPen = new Pen(playerColors[i % playerColors.Length], 2);
                players[i].Draw(g, playerPen);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1150, 600);
            this.Name = "MainForm";
            this.Text = "Super Smash Trees";
            this.ResumeLayout(false);
        }
    }
}