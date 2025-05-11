using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto
{
    public class Player
    {
        public int X, Y;
        public int Width = 20, Height = 40;
        public int SpeedX = 0, SpeedY = 0;
        public int Score = 0;

        private Keys left, right, jump;
        private bool onGround = false;
        private bool jumping = false;

        public Player(int x, int y, Keys left, Keys right, Keys jump)
        {
            X = x; Y = y;
            this.left = left;
            this.right = right;
            this.jump = jump;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public void Update(List<Platform> platforms)
        {
            SpeedY += 1; // gravedad

            X += SpeedX;
            Y += SpeedY;

            onGround = false;

            foreach (var p in platforms)
            {
                if (Bounds.IntersectsWith(p.Bounds))
                {
                    if (SpeedY > 0)
                    {
                        Y = p.Y - Height;
                        SpeedY = 0;
                        onGround = true;
                        jumping = false;
                    }
                }
            }
        }

        public void KeyDown(Keys key)
        {
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
            if (key == left || key == right) SpeedX = 0;
        }

        public void Draw(Graphics g, Pen pen)
        {
            // Dibujar stickman
            int headRadius = 10;
            g.DrawEllipse(pen, X + 5, Y, headRadius * 2, headRadius * 2); // cabeza
            g.DrawLine(pen, X + 15, Y + 20, X + 15, Y + 40); // cuerpo
            g.DrawLine(pen, X + 15, Y + 25, X, Y + 30); // brazo izquierdo
            g.DrawLine(pen, X + 15, Y + 25, X + 30, Y + 30); // brazo derecho
            g.DrawLine(pen, X + 15, Y + 40, X, Y + 60); // pierna izq
            g.DrawLine(pen, X + 15, Y + 40, X + 30, Y + 60); // pierna der
        }
    }
}
