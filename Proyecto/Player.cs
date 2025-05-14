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

    }    
}
