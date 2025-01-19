using System.Drawing;
using Godot;

public struct Hallway
{
    public Vector2 A;
    public Vector2 B;

    public OrientationType Orientation;

    int Width;

    public enum OrientationType
    {
        Vertical,
        Horizontal,
    }

    public Hallway(Vector2 a, Vector2 b)
    {
        // A should always be before B
        if (a.X < b.X || a.Y < b.Y)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }

        Orientation = a.X == b.X ? OrientationType.Vertical : OrientationType.Horizontal;
        Width = 1;
    }

    public Vector2I GridDirection
    {
        get { return (Vector2I)(B - A).Normalized().Round(); }
    }

    public int GridLength
    {
        get
        {
            var length = ((B - A) / Constants.GridSize).Length();
            return (int)Mathf.Round(length);
        }
    }

    public Rect2 GetRect()
    {
        var size = (B - A) / 2;
        if (OrientationType.Vertical == Orientation)
        {
            size.X = Width * Constants.GridSize / 2.0f;
        }
        else
        {
            size.Y = Width * Constants.GridSize / 2.0f;
        }
        return new Rect2(A, size.Abs());
    }
}
