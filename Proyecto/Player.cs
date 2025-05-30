using Proyecto.Entities;
using Proyecto.Inputs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private Keys left, right, jump, powerKey;
        private ControlType controlType;
        private int gamepadIndex;
        private bool onGround = false;
        private bool jumping = false;

        public bool UsingBTree = true;

        private int keyLimit = 20;

        public bool IsUsingBST = false;
        public const int MAX_BTREE_NODES = 15;
        private const int MAX_BST_NODES = 20;

        public BSTree BSTree { get; set; }
        public int TotalInsertedKeys { get; set; }
        public int AVLRotations { get; set; }
        public List<TreeChallenge> CompletedChallenges { get; set; } = new();

        public string CurrentTreeType { get; set; }

        public TreeChallenge ActiveChallenge { get; set; }
        public AVLTree AVLTree { get; set; }
        public bool IsUsingAVL { get; set; } = false;

        public List<TreeChallenge> CurrentChallenges { get; set; } = new List<TreeChallenge>();

        private ChallengeManager challengeManager = new ChallengeManager();

        // Sistema de poderes
        public List<Power> Powers { get; set; } = new List<Power>();
        public int PushEffectTime { get; set; } = 0;
        private PowerManager powerManager = new PowerManager();

        // Control de Air Jump
        private bool hasUsedAirJump = false;

        // Control de botones del gamepad para evitar spam
        private bool xButtonPressed = false;
        private bool yButtonPressed = false;
        private bool bButtonPressed = false;
        private bool aButtonForPowerPressed = false;

        // CORREGIDO: Control de teclas para evitar spam en teclado
        private bool powerKeyPressed = false;
        private bool shiftKeyPressed = false;
        private bool zKeyPressed = false; // Para Force Push
        private bool xKeyPressed = false; // Para Shield  
        private bool cKeyPressed = false; // Para Air Jump

        public BTree Tree;

        public Player(int x, int y, ControlType type, int index = 0, Keys left = Keys.None, Keys right = Keys.None, Keys jump = Keys.None, Keys power = Keys.None)
        {
            X = x;
            Y = y;
            controlType = type;
            gamepadIndex = index;
            this.left = left;
            this.right = right;
            this.jump = jump;
            this.powerKey = power;
            Tree = new BTree(3);
            BSTree = null;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public bool HasShieldActive()
        {
            return Powers.Any(p => p.Type == PowerType.Shield && p.IsActive);
        }

        public bool HasAirJumpPower()
        {
            return Powers.Any(p => p.Type == PowerType.AirJump);
        }

        public Power GetAirJumpPower()
        {
            return Powers.FirstOrDefault(p => p.Type == PowerType.AirJump);
        }

        public bool IsOnGround()
        {
            return onGround;
        }

        public void AddPower(Power power)
        {
            // Permitir múltiples poderes del mismo tipo pero limitar a 3 total
            if (Powers.Count >= 3)
            {
                Powers.RemoveAt(0); // Remover el más antiguo
            }
            Powers.Add(power);
        }

        // Método principal para usar poderes
        public void UsePower(PowerType powerType, List<Player> allPlayers = null)
        {
            var power = Powers.FirstOrDefault(p => p.Type == powerType && p.CanUse());
            if (power == null) return;

            switch (powerType)
            {
                case PowerType.ForcePush:
                    UseForcePush(allPlayers);
                    break;
                case PowerType.Shield:
                    UseShield();
                    break;
                case PowerType.AirJump:
                    UseAirJump();
                    break;
            }
        }

        // Usar el primer poder disponible automáticamente
        public void UseFirstAvailablePower(List<Player> allPlayers = null)
        {
            // Prioridad: ForcePush > Shield > AirJump
            var forcePush = Powers.FirstOrDefault(p => p.Type == PowerType.ForcePush && p.CanUse());
            var shield = Powers.FirstOrDefault(p => p.Type == PowerType.Shield && p.CanUse());
            var airJump = Powers.FirstOrDefault(p => p.Type == PowerType.AirJump && p.CanUse());

            if (forcePush != null)
            {
                UseForcePush(allPlayers);
            }
            else if (shield != null)
            {
                UseShield();
            }
            else if (airJump != null && PowerManager.CanAirJump(this))
            {
                UseAirJump();
            }
        }

        private void UseForcePush(List<Player> allPlayers)
        {
            if (allPlayers == null) return;

            var power = Powers.FirstOrDefault(p => p.Type == PowerType.ForcePush && p.CanUse());
            if (power == null) return;

            // Encontrar jugadores cercanos para empujar
            bool pushedSomeone = false;
            foreach (var otherPlayer in allPlayers)
            {
                if (otherPlayer == this) continue;

                float distance = Math.Abs(otherPlayer.X - this.X);
                if (distance <= 100) // Rango del force push
                {
                    PowerManager.ApplyForcePush(this, otherPlayer, 15);
                    pushedSomeone = true;
                }
            }

            // Solo consumir el poder si efectivamente empujamos a alguien
            if (pushedSomeone)
            {
                Powers.Remove(power);
            }
        }

        private void UseShield()
        {
            var power = Powers.FirstOrDefault(p => p.Type == PowerType.Shield && p.CanUse());
            if (power == null) return;

            power.Activate();
        }

        private void UseAirJump()
        {
            // Verificar condiciones más claramente
            if (onGround || hasUsedAirJump) return;

            var power = Powers.FirstOrDefault(p => p.Type == PowerType.AirJump);
            if (power == null) return;

            // Salto súper alto de emergencia
            SpeedY = -25;
            hasUsedAirJump = true;

            // Remover el poder después de usarlo
            Powers.Remove(power);
        }

        public void Update(List<Platform> platforms, List<Player> allPlayers = null)
        {
            // Actualizar poderes
            foreach (var power in Powers.ToList())
            {
                power.Update();

                // Remover poderes que ya no están activos y han terminado su duración
                if (!power.IsActive && power.Type == PowerType.Shield && power.RemainingTime <= 0)
                {
                    Powers.Remove(power);
                }
            }

            // Actualizar efecto visual de empujón
            if (PushEffectTime > 0)
                PushEffectTime--;

            if (controlType == ControlType.Gamepad)
            {
                HandleGamepadInput(allPlayers);
            }

            SpeedY += 1; // gravedad
            X += SpeedX;
            Y += SpeedY;

            onGround = false;

            foreach (var p in platforms)
            {
                if (Bounds.IntersectsWith(p.Bounds))
                {
                    if (SpeedY > 0 && Y + Height - SpeedY <= p.Y)
                    {
                        Y = p.Y - Height;
                        SpeedY = 0;
                        onGround = true;
                        jumping = false;
                        hasUsedAirJump = false; // Resetear air jump al tocar el suelo
                    }
                }
            }
        }

        // Manejo de gamepad mejorado
        private void HandleGamepadInput(List<Player> allPlayers = null)
        {
            if (XInput.GetState(gamepadIndex, out XInput.GamepadState state) == XInput.ERROR_SUCCESS)
            {
                var gamepad = state.Gamepad;
                SpeedX = 0;

                float normLX = Math.Max(-1, (float)gamepad.sThumbLX / 32767);

                if (normLX < -0.5f)
                    SpeedX = -5;
                else if (normLX > 0.5f)
                    SpeedX = 5;

                // Salto normal
                bool aButtonCurrentlyPressed = (gamepad.wButtons & XInput.XINPUT_GAMEPAD_A) != 0;
                if (aButtonCurrentlyPressed && onGround && !jumping)
                {
                    SpeedY = -15;
                    jumping = true;
                }

                // Control de botones con prevención de spam
                bool xButtonCurrentlyPressed = (gamepad.wButtons & XInput.XINPUT_GAMEPAD_X) != 0;
                bool yButtonCurrentlyPressed = (gamepad.wButtons & XInput.XINPUT_GAMEPAD_Y) != 0;
                bool bButtonCurrentlyPressed = (gamepad.wButtons & XInput.XINPUT_GAMEPAD_B) != 0;

                // Force Push con botón X
                if (xButtonCurrentlyPressed && !xButtonPressed)
                {
                    UsePower(PowerType.ForcePush, allPlayers);
                }
                xButtonPressed = xButtonCurrentlyPressed;

                // Shield con botón Y
                if (yButtonCurrentlyPressed && !yButtonPressed)
                {
                    UsePower(PowerType.Shield, allPlayers);
                }
                yButtonPressed = yButtonCurrentlyPressed;

                // Air Jump con botón B (solo si no está en el suelo)
                if (bButtonCurrentlyPressed && !bButtonPressed && !onGround)
                {
                    UsePower(PowerType.AirJump);
                }
                bButtonPressed = bButtonCurrentlyPressed;

                // Usar primer poder disponible con A cuando no estás en el suelo
                if (aButtonCurrentlyPressed && !aButtonForPowerPressed && !onGround)
                {
                    UseFirstAvailablePower(allPlayers);
                }
                aButtonForPowerPressed = aButtonCurrentlyPressed && !onGround;
            }
        }

        public void KeyDown(Keys key, List<Player> allPlayers = null)
        {
            if (controlType != ControlType.Keyboard) return;

            // Movimiento básico
            if (key == left) SpeedX = -5;
            if (key == right) SpeedX = 5;

            // Salto normal
            if (key == jump && onGround && !jumping)
            {
                SpeedY = -15;
                jumping = true;
            }

            // CORREGIDO: Usar primer poder disponible con tecla de poder principal
            if (key == powerKey && !powerKeyPressed)
            {
                UseFirstAvailablePower(allPlayers);
                powerKeyPressed = true;
            }

            // CORREGIDO: Teclas específicas para cada poder con control de spam

            // Z para Force Push
            if (key == Keys.Z && !zKeyPressed)
            {
                UsePower(PowerType.ForcePush, allPlayers);
                zKeyPressed = true;
            }

            // X para Shield
            if (key == Keys.X && !xKeyPressed)
            {
                UsePower(PowerType.Shield);
                xKeyPressed = true;
            }

            // C para Air Jump (solo si no está en el suelo)
            if (key == Keys.C && !cKeyPressed && !onGround)
            {
                UsePower(PowerType.AirJump);
                cKeyPressed = true;
            }

            // Air Jump alternativo con Shift (solo si no está en el suelo)
            if ((key == Keys.ShiftKey || key == Keys.RShiftKey) && !shiftKeyPressed && !onGround)
            {
                UsePower(PowerType.AirJump);
                shiftKeyPressed = true;
            }
        }

        public void KeyUp(Keys key)
        {
            if (controlType != ControlType.Keyboard) return;

            // Movimiento
            if (key == left || key == right) SpeedX = 0;

            // CORREGIDO: Resetear estado de todas las teclas para evitar spam
            if (key == powerKey) powerKeyPressed = false;
            if (key == Keys.ShiftKey || key == Keys.RShiftKey) shiftKeyPressed = false;
            if (key == Keys.Z) zKeyPressed = false;
            if (key == Keys.X) xKeyPressed = false;
            if (key == Keys.C) cKeyPressed = false;
        }

        // Métodos para el sistema de árboles (sin cambios)
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

                if (BSTree.CountNodes() >= MAX_BST_NODES)
                {
                    var allValues = BSTree.GetAllValues();
                    AVLTree = new AVLTree();
                    foreach (var v in allValues)
                        AVLTree.Insert(v);

                    BSTree = null;
                    IsUsingAVL = true;
                    IsUsingBST = false;
                }

                return;
            }

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

        public void CompleteActiveChallenge()
        {
            if (ActiveChallenge != null)
            {
                CompletedChallenges.Add(ActiveChallenge);

                // Otorgar poder al completar reto
                var newPower = powerManager.GetRandomPower();
                AddPower(newPower);

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

        public void Draw(Graphics g, Pen pen)
        {
            int centerX = X + Width / 2;

            // Efecto visual de Shield más visible
            if (HasShieldActive())
            {
                // Múltiples anillos para efecto más dramático
                g.DrawEllipse(new Pen(Color.Cyan, 6), X - 12, Y - 12, Width + 24, Height + 24);
                g.DrawEllipse(new Pen(Color.LightBlue, 4), X - 8, Y - 8, Width + 16, Height + 16);
                g.DrawEllipse(new Pen(Color.White, 2), X - 4, Y - 4, Width + 8, Height + 8);

                // Añadir brillo interior
                using (Brush shieldBrush = new SolidBrush(Color.FromArgb(50, Color.Cyan)))
                {
                    g.FillEllipse(shieldBrush, X - 8, Y - 8, Width + 16, Height + 16);
                }
            }

            // Efecto visual de empujón más visible
            if (PushEffectTime > 0)
            {
                int pulseSize = (30 - PushEffectTime) * 3;
                g.DrawEllipse(new Pen(Color.Red, 5), X - pulseSize, Y - pulseSize, Width + pulseSize * 2, Height + pulseSize * 2);
                g.DrawEllipse(new Pen(Color.Orange, 3), X - 15, Y - 15, Width + 30, Height + 30);
                g.DrawEllipse(new Pen(Color.Yellow, 2), X - 8, Y - 8, Width + 16, Height + 16);
            }

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

            // Dibujar poderes disponibles
            DrawPowers(g);
        }

        private void DrawPowers(Graphics g)
        {
            if (Powers == null || Powers.Count == 0) return;

            int iconSize = 30;
            int startX = X - 10;
            int startY = Y - iconSize - 10;

            for (int i = 0; i < Powers.Count; i++)
            {
                int iconX = startX + (i * (iconSize + 8));
                PowerManager.DrawPowerIcon(g, Powers[i], iconX, startY, iconSize);
            }
        }

        // Métodos auxiliares para el sistema de árboles
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

        public int CountNodes()
        {
            return Tree != null ? CountNodes(Tree.Root) : 0;
        }

        private int CountNodes(BTreeNode node)
        {
            if (node == null) return 0;
            int count = 1;
            foreach (var child in node.Children)
                count += CountNodes(child);
            return count;
        }
    }
}