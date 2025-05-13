using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto
{
    public class BTreeNode
    {
        public int[] Keys;
        public BTreeNode[] Children;
        public int Degree;
        public int KeyCount;
        public bool IsLeaf;

        public BTreeNode(int degree, bool isLeaf)
        {
            Degree = degree;
            IsLeaf = isLeaf;
            Keys = new int[2 * degree - 1];
            Children = new BTreeNode[2 * degree];
            KeyCount = 0;
        }

        // Método mejorado de visualización para dibujar el árbol B
        public void Traverse(Graphics g, Font font, Brush nodeBrush, Pen nodePen, int x, int y, int horizontalSpacing, int verticalSpacing)
        {
            if (KeyCount == 0) return;

            // Calcula el ancho total de este nodo
            int nodeWidth = Math.Max(KeyCount * 30, 30); // Mínimo 30 pixels de ancho
            int nodeHeight = 30;

            // Dibuja el nodo como un rectángulo redondeado
            g.FillRectangle(nodeBrush, x - nodeWidth / 2, y, nodeWidth, nodeHeight);
            g.DrawRectangle(nodePen, x - nodeWidth / 2, y, nodeWidth, nodeHeight);

            // Dibuja líneas verticales para separar las claves dentro del nodo
            for (int i = 1; i < KeyCount; i++)
            {
                int dividerX = x - nodeWidth / 2 + i * (nodeWidth / KeyCount);
                g.DrawLine(nodePen, dividerX, y, dividerX, y + nodeHeight);
            }

            // Dibuja las claves
            for (int i = 0; i < KeyCount; i++)
            {
                int keyX = x - nodeWidth / 2 + i * (nodeWidth / KeyCount) + (nodeWidth / KeyCount / 2);
                g.DrawString(Keys[i].ToString(), font, Brushes.Black, keyX - 8, y + 5);
            }

            // Si no es hoja, dibuja los hijos recursivamente
            if (!IsLeaf)
            {
                // Calcular el ancho total para los hijos
                int childrenWidth = (KeyCount + 1) * horizontalSpacing;
                int startX = x - childrenWidth / 2 + horizontalSpacing / 2;

                // Dibujar los hijos y conectarlos con líneas
                for (int i = 0; i <= KeyCount; i++)
                {
                    if (Children[i] != null)
                    {
                        int childX = startX + i * horizontalSpacing;
                        int childY = y + verticalSpacing;

                        // Dibuja la línea que conecta al hijo
                        g.DrawLine(nodePen, x, y + nodeHeight, childX, childY);

                        // Dibuja el subárbol del hijo
                        Children[i].Traverse(g, font, nodeBrush, nodePen, childX, childY, horizontalSpacing / 2, verticalSpacing);
                    }
                }
            }
        }

        public void InsertNonFull(int key)
        {
            int i = KeyCount - 1;

            if (IsLeaf)
            {
                while (i >= 0 && Keys[i] > key)
                {
                    Keys[i + 1] = Keys[i];
                    i--;
                }
                Keys[i + 1] = key;
                KeyCount++;
            }
            else
            {
                while (i >= 0 && Keys[i] > key)
                    i--;

                if (Children[i + 1].KeyCount == 2 * Degree - 1)
                {
                    SplitChild(i + 1, Children[i + 1]);

                    if (Keys[i + 1] < key)
                        i++;
                }

                Children[i + 1].InsertNonFull(key);
            }
        }

        public void SplitChild(int i, BTreeNode y)
        {
            BTreeNode z = new BTreeNode(y.Degree, y.IsLeaf);
            z.KeyCount = Degree - 1;

            for (int j = 0; j < Degree - 1; j++)
                z.Keys[j] = y.Keys[j + Degree];

            if (!y.IsLeaf)
            {
                for (int j = 0; j < Degree; j++)
                    z.Children[j] = y.Children[j + Degree];
            }

            y.KeyCount = Degree - 1;

            for (int j = KeyCount; j >= i + 1; j--)
                Children[j + 1] = Children[j];

            Children[i + 1] = z;

            for (int j = KeyCount - 1; j >= i; j--)
                Keys[j + 1] = Keys[j];

            Keys[i] = y.Keys[Degree - 1];
            KeyCount++;
        }
    }

    public class BTree
    {
        public BTreeNode Root;
        public int Degree;

        // Colores y estilos para la visualización
        private Brush nodeBrush = Brushes.LightGreen;
        private Pen nodePen = new Pen(Color.Black, 2);
        private Font nodeFont = new Font("Arial", 12, FontStyle.Bold);

        public BTree(int degree)
        {
            Root = null;
            Degree = degree;
        }

        public void Insert(int key)
        {
            // Animación simple: hacer que el nodo parpadee brevemente
            nodeBrush = Brushes.Yellow;

            if (Root == null)
            {
                Root = new BTreeNode(Degree, true);
                Root.Keys[0] = key;
                Root.KeyCount = 1;
            }
            else
            {
                if (Root.KeyCount == 2 * Degree - 1)
                {
                    BTreeNode s = new BTreeNode(Degree, false);
                    s.Children[0] = Root;
                    s.SplitChild(0, Root);

                    int i = 0;
                    if (s.Keys[0] < key)
                        i++;

                    s.Children[i].InsertNonFull(key);
                    Root = s;
                }
                else
                {
                    Root.InsertNonFull(key);
                }
            }

            // Restaurar el color normal después de un breve momento
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((obj) => {
                nodeBrush = Brushes.LightGreen;
                timer.Dispose();
            }, null, 500, System.Threading.Timeout.Infinite);
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (Root != null)
            {
                // Ajusta estos valores para cambiar el espaciado del árbol
                int horizontalSpacing = 120; // Espacio horizontal entre nodos hermanos
                int verticalSpacing = 70;    // Espacio vertical entre niveles

                Root.Traverse(g, nodeFont, nodeBrush, nodePen, x, y, horizontalSpacing, verticalSpacing);
            }
            else
            {
                // Si el árbol está vacío, muestra un mensaje
                g.DrawString("Árbol B vacío", new Font("Arial", 10), Brushes.Black, x, y);
            }
        }
    }
}