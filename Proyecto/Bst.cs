public class BSTNode
{
    public int Value;
    public BSTNode Left, Right;

    public BSTNode(int value)
    {
        Value = value;
    }

    public void Insert(int value)
    {
        if (value < Value)
        {
            if (Left == null)
                Left = new BSTNode(value);
            else
                Left.Insert(value);
        }
        else
        {
            if (Right == null)
                Right = new BSTNode(value);
            else
                Right.Insert(value);
        }
    }

    public void Draw(Graphics g, int x, int y, int dx = 40, int dy = 50)
    {
        g.FillEllipse(Brushes.LightBlue, x - 15, y - 15, 30, 30);
        g.DrawEllipse(Pens.Black, x - 15, y - 15, 30, 30);
        g.DrawString(Value.ToString(), new Font("Arial", 10), Brushes.Black, x - 10, y - 10);

        if (Left != null)
        {
            g.DrawLine(Pens.Black, x, y, x - dx, y + dy);
            Left.Draw(g, x - dx, y + dy, dx - 5, dy);
        }

        if (Right != null)
        {
            g.DrawLine(Pens.Black, x, y, x + dx, y + dy);
            Right.Draw(g, x + dx, y + dy, dx - 5, dy);
        }
    }
}

public class BST
{
    public BSTNode Root;

    public void Insert(int value)
    {
        if (Root == null)
            Root = new BSTNode(value);
        else
            Root.Insert(value);
    }

    public void Draw(Graphics g, int x, int y)
    {
        Root?.Draw(g, x, y);
    }
}
