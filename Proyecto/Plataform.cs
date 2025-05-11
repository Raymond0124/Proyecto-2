using System.Drawing;

namespace Proyecto
{
    public class Platform
    {
        public int X, Y, Width, Height;

        public Platform(int x, int y, int width, int height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.SaddleBrown, Bounds);
        }
    }
}
