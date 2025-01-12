public static class Constants
{
    public const float GridSize = 16f;
}

public class CollisionLayers
{
    public const uint NONE = 0;
    public const uint ENVIRONMENT = 1 << 0;
    public const uint LADDER = 1 << 1;
    public const uint PLAYER = 1 << 2;
    public const uint ROOM = 1 << 3;
}
