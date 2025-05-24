using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto
{
    public class AVLNode
    {
        public int Key;
        public int Height;
        public AVLNode Left;
        public AVLNode Right;

        public AVLNode(int key)
        {
            Key = key;
            Height = 1;
        }
    }

    public class AVLTree
    {
        public AVLNode Root;

        public void Insert(int key)
        {
            Root = Insert(Root, key);
        }

        private int Height(AVLNode node) => node?.Height ?? 0;

        private int BalanceFactor(AVLNode node) => node == null ? 0 : Height(node.Left) - Height(node.Right);

        private void UpdateHeight(AVLNode node) => node.Height = 1 + Math.Max(Height(node.Left), Height(node.Right));

        private AVLNode RotateRight(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            UpdateHeight(y);
            UpdateHeight(x);
            return x;
        }

        private AVLNode RotateLeft(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            UpdateHeight(x);
            UpdateHeight(y);
            return y;
        }

        private AVLNode Insert(AVLNode node, int key)
        {
            if (node == null)
                return new AVLNode(key);

            if (key < node.Key)
                node.Left = Insert(node.Left, key);
            else if (key > node.Key)
                node.Right = Insert(node.Right, key);
            else
                return node; // Sin duplicados

            UpdateHeight(node);
            int balance = BalanceFactor(node);

            // Rotaciones
            if (balance > 1 && key < node.Left.Key)
                return RotateRight(node);

            if (balance < -1 && key > node.Right.Key)
                return RotateLeft(node);

            if (balance > 1 && key > node.Left.Key)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            if (balance < -1 && key < node.Right.Key)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        public int CountNodes()
        {
            return CountNodes(Root);
        }

        private int CountNodes(AVLNode node)
        {
            if (node == null) return 0;
            return 1 + CountNodes(node.Left) + CountNodes(node.Right);
        }

        // ------------------------------------------------------------
        // 1)  MÉTODO PÚBLICO  Draw  (idéntica firma al del BST)
        // ------------------------------------------------------------
        public void Draw(Graphics g, int x, int y)
        {
            if (Root == null)
            {
                g.DrawString("Árbol AVL vacío", new Font("Arial", 10), Brushes.Black, x, y);
                return;
            }

            float scale = 0.8f * CalculateScale();  // AVL se dibuja un 20% más pequeño
            int baseSpacing = (int)(90 * scale);    // Espaciado horizontal reducido
            int verticalSpacing = (int)(25 * scale); // Espaciado vertical reducido


            Brush nodeBrush = Brushes.LightGreen; // Igual que en BST
            Pen nodePen = new Pen(Color.Black, 1);

            TraverseVisual(g, Root, nodeBrush, nodePen, x, y, baseSpacing, verticalSpacing, 0, scale);
        }

        public void TraverseVisual(Graphics g, AVLNode node, Brush nodeBrush, Pen nodePen, int x, int y, int baseSpacing, int verticalSpacing, int depth, float scale)
        {
            if (node == null) return;

            int nodeHeight = (int)(30 * scale);
            int nodeWidth = (int)(30 * scale);
            int currentX = x - nodeWidth / 2;

            int lineOffset = 4;

            // Dibujo del nodo (idéntico al BST)
            g.FillRectangle(nodeBrush, currentX, y, nodeWidth, nodeHeight);
            g.DrawRectangle(nodePen, currentX, y, nodeWidth, nodeHeight);

            using (Font font = new Font("Arial", 10 * scale, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(node.Key.ToString(), font, Brushes.Black, new RectangleF(currentX, y, nodeWidth, nodeHeight), sf);
            }

            // Espaciado dinámico horizontal por profundidad
            int dynamicSpacing = baseSpacing / (depth + 1);
            int childY = y + verticalSpacing;

            if (node.Left != null)
            {
                int childX = x - dynamicSpacing;

                g.DrawLine(nodePen,
                    x, y + nodeHeight - lineOffset,
                    childX, childY + lineOffset
                );

                TraverseVisual(g, node.Left, nodeBrush, nodePen, childX, childY, baseSpacing, verticalSpacing, depth + 1, scale);
            }

            if (node.Right != null)
            {
                int childX = x + dynamicSpacing;

                g.DrawLine(nodePen,
                    x, y + nodeHeight - lineOffset,
                    childX, childY + lineOffset
                );

                TraverseVisual(g, node.Right, nodeBrush, nodePen, childX, childY, baseSpacing, verticalSpacing, depth + 1, scale);
            }
        }


        // ------------------------------------------------------------
        // 3)  ESCALADO  (idéntico al del BST)
        // ------------------------------------------------------------
        private float CalculateScale()
        {
            int d = Depth(Root);
            if (d > 6) return 0.5f;
            if (d > 4) return 0.65f;
            return 0.8f;
        }
        public int Depth()
        {
            return Depth(Root);
        }
        public int Depth(AVLNode n) =>
            n == null ? 0 : 1 + Math.Max(Depth(n.Left), Depth(n.Right));

    }

}
