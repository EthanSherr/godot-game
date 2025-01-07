using Godot;

public static class VectorExtensions
{
    public static Vector2 Truncate(this Vector2 v)
    {
        return new Vector2(MathUtils.Truncate(v.X), MathUtils.Truncate(v.Y));
    }
}
