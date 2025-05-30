using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public int GetDepth()
        {
            int maxChildDepth = 0;
            foreach (var child in Children)
            {
                if (child != null)
                    maxChildDepth = Math.Max(maxChildDepth, child.GetDepth());
            }
            return 1 + maxChildDepth;
        }


        // visualización para dibujar el árbol B
        public void Traverse(Graphics g, Brush nodeBrush, Pen nodePen, int x, int y, int horizontalSpacing, int verticalSpacing, int depth, float scale)


        {

            if (KeyCount == 0) return;




            float scaleFactor = 1.0f - depth * 0.1f; // Disminuye 10% por nivel
            if (scaleFactor < 0.5f) scaleFactor = 0.5f; // No más pequeño que 50%

            Font adjustedFont = new Font("Arial", 12 * scale, FontStyle.Bold);
            int nodeHeight = (int)(30 * scale);
            int baseNodeWidth = 35;
            int nodeWidth = Math.Max((int)(KeyCount * baseNodeWidth * scale), 30);
            ;
            int currentX = x - nodeWidth / 2;

            // Dibuja el nodo actual
            g.FillRectangle(nodeBrush, currentX, y, nodeWidth, nodeHeight);
            g.DrawRectangle(nodePen, currentX, y, nodeWidth, nodeHeight);

            for (int i = 1; i < KeyCount; i++)
            {
                int dividerX = currentX + i * (nodeWidth / KeyCount);
                g.DrawLine(nodePen, dividerX, y, dividerX, y + nodeHeight);
            }

            // Ajustar tamaño de fuente si hay muchas claves// Ajustar tamaño de fuente dinámicamente según el número de claves



            for (int i = 0; i < KeyCount; i++)
            {
                int keyX = x - nodeWidth / 2 + i * (nodeWidth / KeyCount) + (nodeWidth / KeyCount / 2);
                g.DrawString(Keys[i].ToString(), adjustedFont, Brushes.Black, keyX - 8, y + 5);
            }


            // Dibujar hijos
            if (!IsLeaf)
            {
                int totalWidth = GetSubtreeWidth(horizontalSpacing);
                int childX = x - totalWidth / 2;

                for (int i = 0; i <= KeyCount; i++)
                {
                    if (Children[i] != null)
                    {
                        int subTreeWidth = Children[i].GetSubtreeWidth(horizontalSpacing);
                        int childCenterX = childX + subTreeWidth / 2;
                        int childY = y + verticalSpacing;

                        // Línea hacia hijo
                        g.DrawLine(nodePen, x, y + nodeHeight, childCenterX, childY);

                        // Llamada recursiva
                        Children[i].Traverse(g, nodeBrush, nodePen, childCenterX, childY, Math.Max(horizontalSpacing / 2, 100), verticalSpacing, depth + 1, scale);



                        childX += subTreeWidth;
                    }
                    else
                    {
                        childX += horizontalSpacing; // espacio vacío para hijos nulos
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

        public int GetSubtreeWidth(int horizontalSpacing)
        {
            if (IsLeaf || Children.All(c => c == null))
                return Math.Max(KeyCount * 30, 30); // ancho mínimo de nodo

            int width = 0;
            for (int i = 0; i <= KeyCount; i++)
            {
                if (Children[i] != null)
                {
                    width += Children[i].GetSubtreeWidth(horizontalSpacing);
                }
                else
                {
                    width += horizontalSpacing; // espacio en blanco para hijos nulos
                }
            }
            return width;
        }


    }

    public class BTree
    {
        public BTreeNode Root;
        public int Degree;

        // Colores y estilos para la visualización
        private Brush nodeBrush = Brushes.LightGreen;
        private Pen nodePen = new Pen(Color.Black, 2);
        private Font nodeFont = new Font("Arial", 7, FontStyle.Bold);


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
            timer = new System.Threading.Timer((obj) =>
            {
                nodeBrush = Brushes.LightGreen;
                timer.Dispose();
            }, null, 500, System.Threading.Timeout.Infinite);
        }

        private float CalculateScale()
        {
            if (Root == null)
                return 1.0f;

            // Escala basada en el número total de claves o niveles
            int totalKeys = CountKeys(Root);
            int maxKeysBeforeScaling = 10; // se puede ajustar este número

            // Escala siempre comienza más pequeña, y se reduce más si crece
            float scale = 0.6f; // escala inicial más pequeña (se puede ajustar)

            if (totalKeys > maxKeysBeforeScaling)
            {
                scale -= 0.03f * (totalKeys - maxKeysBeforeScaling); // disminuye 3% por clave extra
                if (scale < 0.3f) scale = 0.3f; // mínimo 30% del tamaño original
            }


            return scale;
        }

        public int CountNodes()
        {
            return CountNodes(Root);
        }

        private int CountNodes(BTreeNode node)
        {
            if (node == null) return 0;
            int count = node.Keys.Count();
            foreach (var child in node.Children)
                count += CountNodes(child);
            return count;
        }

        public List<int> InOrderTraversal()
        {
            List<int> result = new List<int>();
            InOrderTraversal(Root, result);
            return result;
        }

        private void InOrderTraversal(BTreeNode node, List<int> result)
        {
            if (node == null) return;
            for (int i = 0; i < node.Keys.Count(); i++)
            {
                if (i < node.Children.Count())
                    InOrderTraversal(node.Children[i], result);
                result.Add(node.Keys[i]);
            }
            if (node.Children.Count() > node.Keys.Count())
                InOrderTraversal(node.Children[^1], result);
        }
        public List<int> GetAllValues()
        {
            var values = new List<int>();
            CollectValues(Root, values);
            return values;
        }

        private void CollectValues(BTreeNode node, List<int> values)
        {
            if (node == null) return;

            for (int i = 0; i < node.Keys.Count(); i++)
            {
                if (!node.IsLeaf)
                    CollectValues(node.Children[i], values);

                values.Add(node.Keys[i]);
            }

            if (!node.IsLeaf)
                CollectValues(node.Children[node.Keys.Count()], values);
        }



        private int CountKeys(BTreeNode node)
        {
            if (node == null) return 0;

            int count = node.KeyCount;

            for (int i = 0; i <= node.KeyCount; i++)
            {
                if (node.Children[i] != null)
                    count += CountKeys(node.Children[i]);
            }

            return count;
        }
        


        public void Draw(Graphics g, int x, int y)
        {
            float scale = CalculateScale();

            if (Root != null)
            {
                int horizontalSpacing = (int)(180 * scale);
                int verticalSpacing = (int)(70 * scale);

                Root.Traverse(g, nodeBrush, nodePen, x, y, horizontalSpacing, verticalSpacing, 0, scale);
            }
            else
            {
                g.DrawString("Árbol B vacío", new Font("Arial", 10), Brushes.Black, x, y);
            }
        }
        public int Depth()
        {
            return Root != null ? Root.GetDepth() : 0;
        }

    }
}