using System.Drawing;

namespace Proyecto
{
    public class Token
    {
        public int X, Y, Value;
        public bool Collected = false;
        private Color tokenColor;

        public Token(int x, int y, int value)
        {
            X = x;
            Y = y;
            Value = value;

            // Asignar color basado en el valor
            if (value < 30)
                tokenColor = Color.Gold;
            else if (value < 60)
                tokenColor = Color.Orange;
            else
                tokenColor = Color.Crimson;
        }

        public Rectangle Bounds => new Rectangle(X, Y, 30, 30);

        public void Draw(Graphics g)
        {
            if (!Collected)
            {
                // Dibujar el token con color basado en su valor
                g.FillEllipse(new SolidBrush(tokenColor), Bounds);
                g.DrawEllipse(Pens.Black, Bounds);

                // Mostrar el valor del token
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    // Texto con sombra para mejorar visibilidad
                    g.DrawString(Value.ToString(), font, Brushes.Black, new RectangleF(X + 1, Y + 1, 30, 30), sf);
                    g.DrawString(Value.ToString(), font, Brushes.White, new RectangleF(X, Y, 30, 30), sf);
                }

                // Efecto de brillo
                g.FillEllipse(new SolidBrush(Color.FromArgb(80, Color.White)), X + 5, Y + 5, 8, 8);
            }
        }
    }
}