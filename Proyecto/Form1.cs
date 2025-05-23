using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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

        private const int MaxBTreeNodes = 15; // límite de nodos del B-tree

        private const int MaxBSTNodes = 5;

        // Nueva variable para animar la construcción del árbol
        private bool treeUpdated = false;

        // Panel para mostrar árboles B
        private Panel treePanel;

        // Colores para los jugadores
        private Color[] playerColors = {
            Color.RoyalBlue,
            Color.Crimson,
            Color.ForestGreen,
            Color.DarkOrange
        };

        // Nuevo: Registro de la última colisión para saber quién empujó a quién
        private Dictionary<Player, Player> lastPlayerContact = new Dictionary<Player, Player>();

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // Configurar el panel de árboles
            ConfigureTreePanel();

            // Inicializar jugadores
            players.Add(new Player(100, 300, ControlType.Keyboard, 0, Keys.A, Keys.D, Keys.W));       // Jugador 1 (WASD)
            players.Add(new Player(700, 300, ControlType.Keyboard, 0, Keys.Left, Keys.Right, Keys.Up)); // Jugador 2 (Flechas)
            players.Add(new Player(400, 100, ControlType.Gamepad, 0)); // Jugador 3 (control 1)
            players.Add(new Player(500, 100, ControlType.Gamepad, 1)); // Jugador 4 (control 2)

            // Inicializar el registro de contactos por jugador
            foreach (var player in players)
            {
                lastPlayerContact[player] = null;
            }

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

                    var winner = players.OrderByDescending(p => p.Score).First();
                    int maxScore = winner.Score;
                    if (players.Count(p => p.Score == maxScore) > 1)
                        MessageBox.Show("¡Empate!");
                    else
                        MessageBox.Show($"¡Ganador: P{players.IndexOf(winner) + 1} con {maxScore} puntos!");

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
            // Crear un panel para mostrar los árboles B
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

            // Dibujar título del panel
            g.DrawString("Árboles B en Tiempo Real", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, 10, 10);

            // Dibujar línea separadora
            g.DrawLine(Pens.Gray, 0, 40, treePanel.Width, 40);

            // Área para dibujar árboles B de ejemplo

            // Dibujar árboles de cada jugador
            int y = 250;
            for (int i = 0; i < players.Count; i++)
            {
                // Sólo mostrar si el jugador tiene tokens recolectados
                if (players[i].Score > 0)
                {
                    string playerName = $"P{i + 1}: {players[i].Score} pts";
                    g.DrawString(playerName, new Font("Arial", 10, FontStyle.Bold),
                                new SolidBrush(playerColors[i % playerColors.Length]), 10, y - 200);

                    // Dibujar el árbol B del jugador
                    if (players[i].IsUsingBST)
                    {
                        DrawCompactTree(g, players[i], 20, y, playerColors[i % playerColors.Length]);
                    }
                    else
                    {
                        DrawCompactTree(g, players[i], 20, y, playerColors[i % playerColors.Length]);
                    }

                    y += 135; // Espacio vertical entre árboles de jugadores
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

        private void DrawCompactTree(Graphics g, Player player, int x, int y, Color color)
        {
            if (player.IsUsingAVL && player.AVLTree != null)
            {
                player.AVLTree.Draw(g, x + 180, y - 180); // Este método dibuja el árbol con nodos modernos
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

            // Plataforma principal centrada
            int mainPlatformWidth = 500;
            int mainPlatformX = (this.ClientSize.Width - treePanel.Width - mainPlatformWidth) / 2;
            platforms.Add(new Platform(mainPlatformX, this.ClientSize.Height - 50, mainPlatformWidth, 20));

            int maxWidth = this.ClientSize.Width - treePanel.Width;
            int maxHeight = this.ClientSize.Height;
            // Define zonas de aparición para las plataformas aleatorias (en porcentaje del área jugable)
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
        private void ShowKOMessage(Player winner, Player loser)
        {
            Label label = new Label
            {
                Text = $"¡P{players.IndexOf(winner) + 1} KO a P{players.IndexOf(loser) + 1}!",
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(300, 100)
            };
            this.Controls.Add(label);

            Timer removeLabelTimer = new Timer { Interval = 2000 };
            removeLabelTimer.Tick += (s, e) =>
            {
                this.Controls.Remove(label);
                removeLabelTimer.Stop();
            };
            removeLabelTimer.Start();
        }

        




        private void GameLoop(object sender, EventArgs e)
        {
            foreach (var player in players)
                player.Update(platforms);

            foreach (var player in players)
            {
                if (player.Y > this.ClientSize.Height)
                {
                    // Nuevo: Si un jugador cae, verificar si alguien lo empujó
                    Player pusher = lastPlayerContact[player];
                    if (pusher != null)
                    {
                        // Otorgar 2 puntos al empujador (puedes ajustar el valor)
                        pusher.Score += 2;
                        
                        // Actualizar el árbol del empujador con un valor aleatorio entre 1-100
                        int pushValue = rnd.Next(1, 100);
                        if (!pusher.IsUsingBST)
                        {
                            pusher.InsertKey(pushValue);
                            
                            if (pusher.Tree.CountNodes() > MaxBTreeNodes)
                            {
                                // Convertir a BST
                                pusher.Tree = null;
                                pusher.BSTree = new BSTree(); // BST limpio, vacío
                                pusher.IsUsingBST = true;
                                treeUpdated = true;
                            }
                        }
                        else
                        {
                            pusher.BSTree.Insert(pushValue);

                            if (pusher.BSTree.CountNodes() > MaxBSTNodes && !pusher.IsUsingAVL)
                            {
                                // Convertir a AVL
                                var bstValues = pusher.BSTree.InOrderTraversal(); // Necesitas este método
                                pusher.AVLTree = new AVLTree();
                                foreach (var v in bstValues)
                                    pusher.AVLTree.Insert(v);

                                pusher.BSTree = null;
                                pusher.IsUsingAVL = true;
                            }

                        }

                        treePanel.Invalidate();
                        
                        // Mostrar mensaje de KO en pantalla (puedes implementar esto)
                        // ShowKOMessage(pusher, player);
                    }
                    
                    // Restablecer el último contacto
                    lastPlayerContact[player] = null;
                    
                    // Respawn del jugador caído
                    RespawnPlayer(player);
                }
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
                            // Registrar el contacto entre jugadores
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
                        anyTokenCollected = true;

                        if (!player.IsUsingBST)
                        {
                            player.InsertKey(token.Value);

                            if (player.Tree.CountNodes() > MaxBTreeNodes)
                            {
                                player.Tree = null;
                                player.BSTree = new BSTree(); // BST vacío
                                player.IsUsingBST = true;
                                treeUpdated = true;
                                treePanel.Invalidate();
                            }
                        }
                        else if (!player.IsUsingAVL)
                        {
                            if (player.BSTree != null)
                            {
                                player.BSTree.Insert(token.Value);

                                if (player.BSTree.CountNodes() > MaxBSTNodes)
                                {
                                    player.BSTree = null;
                                    player.AVLTree = new AVLTree(); // AVL vacío
                                    player.IsUsingAVL = true;
                                    treeUpdated = true;
                                    treePanel.Invalidate();
                                }
                            }
                        }
                        else
                        {
                            // Ya está en AVL
                            if (player.AVLTree != null)
                                player.AVLTree.Insert(token.Value);
                        }

                        treePanel.Invalidate();
                        break;
                    }
                }
            }

            // Limpiar tokens colectados
            tokens.RemoveAll(t => t.Collected);

            // Si se recogió algún token, generar uno nuevo después de un pequeño retraso
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

            // Dibujar jugadores con colores específicos
            for (int i = 0; i < players.Count; i++)
            {
                Pen playerPen = new Pen(playerColors[i % playerColors.Length], 2);
                players[i].Draw(g, playerPen);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1150, 600);  // Ventana más ancha para el panel lateral
            this.Name = "MainForm";
            this.Text = "Super Smash Trees";
            this.ResumeLayout(false);
        }
    }
}