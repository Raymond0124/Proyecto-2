using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto
{
    public enum ControlType { Keyboard, Gamepad }

    public class Player
    {
        public int X, Y;
        public int Width = 30, Height = 60;
        public int SpeedX = 0, SpeedY = 0;
        public int Score = 0;

        private Keys left, right, jump;
        private ControlType controlType;
        private int gamepadIndex;
        private bool onGround = false;
        private bool jumping = false;

        public bool UsingBTree = true;
        
        private int keyLimit = 20; // Límite de claves para cambiar de BTree a BST

        public bool IsUsingBST = false;
        public const int MAX_BTREE_NODES = 15;
        private const int MAX_BST_NODES = 20;

        public BSTree BSTree { get; set; }
        public int TotalInsertedKeys { get; set; }
        public int AVLRotations { get; set; }
        public List<TreeChallenge> CompletedChallenges { get; set; } = new();


        public string CurrentTreeType { get; set; } // "BTree", "BST", "AVL"

        public TreeChallenge ActiveChallenge { get; set; }
        public AVLTree AVLTree { get; set; }
        public bool IsUsingAVL { get; set; } = false;

        public List<TreeChallenge> CurrentChallenges { get; set; } = new List<TreeChallenge>();

        private ChallengeManager challengeManager = new ChallengeManager();
        

        private void ConvertToBST()
        {
            BSTree = new BSTree();
            foreach (var v in Tree.InOrderTraversal())  // Debes implementar esto en BTree
            {
                BSTree.Insert(v);
            }
            Tree = null;
            IsUsingBST = true;
        }


        public BTree Tree; // nuevo

        // ✅ Único constructor
        public Player(int x, int y, ControlType type, int index = 0, Keys left = Keys.None, Keys right = Keys.None, Keys jump = Keys.None)
        {
            X = x;
            Y = y;
            controlType = type;
            gamepadIndex = index;
            this.left = left;
            this.right = right;
            this.jump = jump;
            Tree = new BTree(3); // Grado 3 para empezar
            BSTree = null;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public void Update(List<Platform> platforms)
        {
            if (controlType == ControlType.Gamepad)
            {
                if (XInput.GetState(gamepadIndex, out XInput.GamepadState state) != XInput.ERROR_SUCCESS)
                    Console.WriteLine($"[Player] Control {gamepadIndex} NO conectado");
                else
                    Console.WriteLine($"[Player] Control {gamepadIndex} conectado");

                HandleGamepadInput();
            }

            SpeedY += 1; // gravedad
            X += SpeedX;
            Y += SpeedY;

            onGround = false;

            foreach (var p in platforms)
            {
                if (Bounds.IntersectsWith(p.Bounds))
                {
                    // colisión desde arriba
                    if (SpeedY > 0 && Y + Height - SpeedY <= p.Y)
                    {
                        Y = p.Y - Height;
                        SpeedY = 0;
                        onGround = true;
                        jumping = false;
                    }
                }
            }
        }

        public void SetNextActiveChallenge()
        {
            var remaining = CurrentChallenges.Except(CompletedChallenges).ToList();
            if (remaining.Count > 0)
                ActiveChallenge = remaining[new Random().Next(remaining.Count)];
            else
                ActiveChallenge = null;
        }
        public void DrawChallenges(Graphics g)
        {
            if (ActiveChallenge == null) return;

            int startX = X - 10;
            int startY = Y - 40;

            using (Font font = new Font("Arial", 8))
            using (Brush brush = Brushes.White)
            {
                string text = ActiveChallenge.Description;
                bool completed = ActiveChallenge.Condition(this);
                Brush textBrush = completed ? Brushes.LightGreen : brush;
                g.DrawString(text, font, textBrush, startX, startY);
            }
        }
        

        public void StartTree(string treeType)
        {
            CurrentTreeType = treeType;
            TotalInsertedKeys = 0;
            AVLRotations = 0;

            CurrentChallenges = challengeManager.GetChallengesFor(treeType);
            CompletedChallenges = new List<TreeChallenge>();
            SetNextActiveChallenge();

        }

        public bool HasCompletedActiveChallenge()
        {
            return CurrentChallenges != null && CurrentChallenges.All(ch => ch.Condition(this));
        }

        private TreeChallenge GetRandomChallenge(List<TreeChallenge> challenges)
        {
            Random rand = new();
            return challenges[rand.Next(challenges.Count)];
        }

        private void HandleGamepadInput()
        {
            if (XInput.GetState(gamepadIndex, out XInput.GamepadState state) == XInput.ERROR_SUCCESS)
            {
                var gamepad = state.Gamepad;
                SpeedX = 0;

                float normLX = Math.Max(-1, (float)gamepad.sThumbLX / 32767);
                Console.WriteLine($"[Player] ThumbLX: {gamepad.sThumbLX}, normLX: {normLX}");

                if (normLX < -0.5f)
                    SpeedX = -5;
                else if (normLX > 0.5f)
                    SpeedX = 5;

                if ((gamepad.wButtons & XInput.XINPUT_GAMEPAD_A) != 0 && onGround && !jumping)
                {
                    SpeedY = -15;
                    jumping = true;
                }
            }
        }

        public void KeyDown(Keys key)
        {
            if (controlType != ControlType.Keyboard) return;

            if (key == left) SpeedX = -5;
            if (key == right) SpeedX = 5;
            if (key == jump && onGround && !jumping)
            {
                SpeedY = -15;
                jumping = true;
            }
        }

        public void KeyUp(Keys key)
        {
            if (controlType != ControlType.Keyboard) return;

            if (key == left || key == right) SpeedX = 0;
        }
        public void InsertKey(int value)
        {
            if (IsUsingAVL)
            {
                AVLTree.Insert(value);
                return;
            }

            if (IsUsingBST)
            {
                BSTree.Insert(value);

                // Verificar si ya debe convertirse a AVL
                if (BSTree.CountNodes() >= MAX_BST_NODES)
                {
                    var allValues = BSTree.GetAllValues(); // Debes implementar este método en BSTree.cs
                    AVLTree = new AVLTree();
                    foreach (var v in allValues)
                        AVLTree.Insert(v);

                    BSTree = null;
                    IsUsingAVL = true;
                    IsUsingBST = false;
                }

                return;
            }

            // Caso inicial: usando BTree
            Tree.Insert(value);
            if (CountTotalKeys(Tree.Root) >= keyLimit)
            {
                var allKeys = new List<int>();
                CollectKeys(Tree.Root, allKeys);

                BSTree = new BSTree();
                foreach (var k in allKeys)
                    BSTree.Insert(k);

                Tree = null;
                IsUsingBST = true;
            }

            if (ActiveChallenge != null && ActiveChallenge.Condition(this))
            {
                CompleteActiveChallenge();


                if (CompletedChallenges.Count >= 3)
                {
                    // avanzar al siguiente tipo de árbol
                    if (CurrentTreeType == "BTree")
                    {
                        StartTree("BST");
                        BSTree = new BSTree();
                        IsUsingBST = true;
                        UsingBTree = false;
                    }
                    else if (CurrentTreeType == "BST")
                    {
                        StartTree("AVL");
                        AVLTree = new AVLTree();
                        IsUsingAVL = true;
                        IsUsingBST = false;
                    }
                    else if (CurrentTreeType == "AVL")
                    {
                        MessageBox.Show("¡Felicidades! Completaste todos los retos.");
                    }
                }
            }


        }



        private int CountTotalKeys(BTreeNode node)
        {
            if (node == null) return 0;
            int count = node.KeyCount;
            for (int i = 0; i <= node.KeyCount; i++)
            {
                count += CountTotalKeys(node.Children[i]);
            }
            return count;
        }

        private void CollectKeys(BTreeNode node, List<int> keys)
        {
            if (node == null) return;
            for (int i = 0; i < node.KeyCount; i++)
            {
                if (!node.IsLeaf) CollectKeys(node.Children[i], keys);
                keys.Add(node.Keys[i]);
            }
            if (!node.IsLeaf) CollectKeys(node.Children[node.KeyCount], keys);
        }

        public void Draw(Graphics g, Pen pen)
        {
            int centerX = X + Width / 2;

            // Cabeza
            g.DrawEllipse(pen, centerX - 10, Y, 20, 20);

            // Cuerpo
            g.DrawLine(pen, centerX, Y + 20, centerX, Y + 40);

            // Brazos
            g.DrawLine(pen, centerX, Y + 25, X, Y + 30);
            g.DrawLine(pen, centerX, Y + 25, X + Width, Y + 30);

            // Piernas
            g.DrawLine(pen, centerX, Y + 40, X, Y + 60);
            g.DrawLine(pen, centerX, Y + 40, X + Width, Y + 60);
        }

        public int CountNodes()
        {
            return Tree != null ? CountNodes(Tree.Root) : 0;
        }
        public void CompleteActiveChallenge()
        {
            if (ActiveChallenge != null)
            {
                CompletedChallenges.Add(ActiveChallenge);

                // Reiniciar el árbol según el tipo actual
                switch (CurrentTreeType)
                {
                    case "BTree":
                        Tree = new BTree(3);
                        break;
                    case "BST":
                        BSTree = new BSTree();
                        break;
                    case "AVL":
                        AVLTree = new AVLTree();
                        break;
                }

                SetNextActiveChallenge();
            }
        }

        private int CountNodes(BTreeNode node)
        {
            if (node == null) return 0;
            int count = 1; // contar el nodo actual
            foreach (var child in node.Children)
                count += CountNodes(child);
            return count;
        }

    }

    

}
