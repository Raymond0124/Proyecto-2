using System.Drawing;

namespace Proyecto
{
    public class Token
    {
        public int X, Y, Value;
        public bool Collected = false;

        public Token(int x, int y, int value)
        {
            X = x; Y = y; Value = value;
        }

        public Rectangle Bounds => new Rectangle(X, Y, 20, 20);

        public void Draw(Graphics g)
        {
            if (!Collected)
                g.FillEllipse(Brushes.Gold, Bounds);
        }
    }
}
