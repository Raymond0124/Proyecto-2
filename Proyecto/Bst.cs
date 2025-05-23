using Proyecto; // o el namespace donde esté tu BinarySearchTree

namespace Proyecto
{
    public class BSTNode
    {
        public int Value;
        public BSTNode Left, Right;

        public BSTNode(int value)
        {
            Value = value;
        }

        public void Traverse(Graphics g, int x, int y, int hSpacing, int vSpacing, float scale)
        {
            // Dibuja el nodo (igual a los nodos del BTree)
            Rectangle rect = new Rectangle(x - 15, y - 15, 30, 30);
            using (Brush fillBrush = new SolidBrush(Color.LightBlue))
            using (Pen borderPen = new Pen(Color.Black))
            using (Font font = new Font("Arial", 8))
            using (StringFormat format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.FillEllipse(fillBrush, rect);
                g.DrawEllipse(borderPen, rect);
                g.DrawString(Value.ToString(), font, Brushes.Black, rect, format);
            }

            // Línea e hijo izquierdo
            if (Left != null)
            {
                g.DrawLine(Pens.Black, x, y, x - hSpacing, y + vSpacing);
                Left.Traverse(g, x - hSpacing, y + vSpacing, hSpacing / 2, vSpacing, scale);
            }

            // Línea e hijo derecho
            if (Right != null)
            {
                g.DrawLine(Pens.Black, x, y, x + hSpacing, y + vSpacing);
                Right.Traverse(g, x + hSpacing, y + vSpacing, hSpacing / 2, vSpacing, scale);
            }
        }







    }

    public class BSTree
    {
        public BSTNode Root;

        public void Insert(int value)
        {
            Root = Insert(Root, value);
        }

        private BSTNode Insert(BSTNode node, int value)
        {
            if (node == null) return new BSTNode(value);
            if (value < node.Value) node.Left = Insert(node.Left, value);
            else if (value > node.Value) node.Right = Insert(node.Right, value);
            return node;
        }



        private void DrawNode(Graphics g, BSTNode node, int x, int y, int offset)
        {
            if (node == null) return;

            g.FillEllipse(Brushes.White, x - 15, y - 15, 30, 30);
            g.DrawEllipse(Pens.Black, x - 15, y - 15, 30, 30);
            g.DrawString(node.Value.ToString(), new Font("Arial", 8), Brushes.Black, x - 10, y - 8);

            if (node.Left != null)
            {
                g.DrawLine(Pens.Black, x, y, x - offset, y + 50);
                DrawNode(g, node.Left, x - offset, y + 50, offset / 2);
            }

            if (node.Right != null)
            {
                g.DrawLine(Pens.Black, x, y, x + offset, y + 50);
                DrawNode(g, node.Right, x + offset, y + 50, offset / 2);
            }
        }

        public void Draw(Graphics g, int x, int y)
        {
            if (Root == null)
            {
                g.DrawString("Árbol BST vacío", new Font("Arial", 10), Brushes.Black, x, y);
                return;
            }

            float scale = 0.8f;
            int baseSpacing = (int)(130 * scale); // Espaciado inicial más amplio
            int verticalSpacing = (int)(30 * scale); // Vertical sigue siendo reducido

            Brush nodeBrush = Brushes.LightGreen;
            Pen nodePen = new Pen(Color.Black, 1);

            TraverseVisual(g, Root, nodeBrush, nodePen, x, y, baseSpacing, verticalSpacing, 0, scale);
        }

        public void TraverseVisual(Graphics g, BSTNode node, Brush nodeBrush, Pen nodePen, int x, int y, int baseSpacing, int verticalSpacing, int depth, float scale)
        {
            if (node == null) return;

            int nodeHeight = (int)(20 * scale);
            int nodeWidth = (int)(20 * scale);
            int currentX = x - nodeWidth / 2;

            int lineOffset = 4;

            g.FillRectangle(nodeBrush, currentX, y, nodeWidth, nodeHeight);
            g.DrawRectangle(nodePen, currentX, y, nodeWidth, nodeHeight);

            using (Font font = new Font("Arial", 10 * scale, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(node.Value.ToString(), font, Brushes.Black, new RectangleF(currentX, y, nodeWidth, nodeHeight), sf);
            }

            // Cálculo dinámico del espaciado
            int dynamicSpacing = baseSpacing / (depth + 1); // reduce a mayor profundidad

            if (node.Left != null)
            {
                int childX = x - dynamicSpacing;
                int childY = y + verticalSpacing;

                g.DrawLine(nodePen,
                    x, y + nodeHeight - lineOffset,
                    childX, childY + lineOffset
                );

                TraverseVisual(g, node.Left, nodeBrush, nodePen, childX, childY, baseSpacing, verticalSpacing, depth + 1, scale);
            }

            if (node.Right != null)
            {
                int childX = x + dynamicSpacing;
                int childY = y + verticalSpacing;

                g.DrawLine(nodePen,
                    x, y + nodeHeight - lineOffset,
                    childX, childY + lineOffset
                );

                TraverseVisual(g, node.Right, nodeBrush, nodePen, childX, childY, baseSpacing, verticalSpacing, depth + 1, scale);
            }
        }


        public List<int> GetAllValues()
        {
            var values = new List<int>();
            InOrder(Root, values);
            return values;
        }

        public void InOrder(BSTNode node, List<int> values)
        {
            if (node == null) return;
            InOrder(node.Left, values);
            values.Add(node.Value);
            InOrder(node.Right, values);
        }


        public int CountNodes()
        {
            return CountNodes(Root);
        }

        private int CountNodes(BSTNode node)
        {
            if (node == null) return 0;
            return 1 + CountNodes(node.Left) + CountNodes(node.Right);
        }


        public List<int> InOrderTraversal()
        {
            var list = new List<int>();
            InOrder(Root, list);
            return list;
        }

        private void InOrderRecursive(BSTNode node, List<int> list)
{
    if (node == null) return;
    InOrderRecursive(node.Left, list);
    list.Add(node.Value);
    InOrderRecursive(node.Right, list);
}

        private int Count(BSTNode node)
        {
            if (node == null) return 0;
            return 1 + Count(node.Left) + Count(node.Right);
        }


        private float CalculateScale()
        {
            // Escalado simple basado en profundidad o tamaño del árbol
            int depth = GetDepth(Root);
            return depth > 4 ? 0.6f : 1.0f;
        }

        private int GetDepth(BSTNode node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetDepth(node.Left), GetDepth(node.Right));
        }

    }
}