using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class RoomGenerator : Node2D
{
    // normal random
    float meanWidth = 5f;
    float stddevWidth = 2f;
    float meanHeight = 4f;
    float stddevHeight = 2f;

    float Dim = Constants.GridSize;

    // random
    Vector2I MinDims = new Vector2I(5, 5);
    Vector2I MaxDims = new Vector2I(125, 100);

    float Radius = 300;

    [Export]
    private ulong Seed = 0;

    private RandomNumberGenerator rng;

    private bool slowly = true;

    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Seed = Seed;

        GenerateDungeon();
    }

    public async void GenerateDungeon()
    {
        AddChild(makeGrid());

        var rooms = await GenerateRooms();
        await Separate(rooms);
        await Snap(rooms);
        GD.Print("separate again");
        await Separate(rooms);
        await Snap(rooms);

        var selectedRooms = await SelectRooms(rooms);
        var edges = await RelateSelectedRooms(selectedRooms);
        var hallways = await AddHallways(edges);
        await IntersectRooms(hallways);
    }

    public Node2D makeGrid()
    {
        var dd = new DebugDrawer { Thickness = 1f };
        var gridLines = 100;
        var lineColor = new Color(0, 0, 0);
        dd.Position = -0.5f * Dim * new Vector2(gridLines, gridLines);
        for (var x = 0; x < gridLines; x++)
        {
            var start = new Vector2(x * Dim, 0);
            var end = new Vector2(x * Dim, gridLines * Dim);
            dd.AddLine(start, end, lineColor);
        }

        for (var y = 0; y < gridLines; y++)
        {
            var start = new Vector2(0, y * Dim);
            var end = new Vector2(gridLines * Dim, y * Dim);
            dd.AddLine(start, end, lineColor);
        }
        return dd;
    }

    public async Task<List<RoomVisualizer>> GenerateRooms()
    {
        List<RoomVisualizer> rooms = new List<RoomVisualizer>();
        for (int i = 0; i < 150; i++)
        {
            Vector2 dimension = GenerateNormalRadomRoomSize();
            RoomVisualizer room = new RoomVisualizer
            {
                Size = dimension,
                Dim = Dim,
                BorderColor = new Color(0, 0, 1),
                FillColor = new Color(0, 0, 0.8f, 0.5f),
                BorderThickness = 2,
                Id = i,
            };
            room.Position = MathUtils.RandomPointInCircle(Radius, rng);
            AddChild(room);

            rooms.Add(room);
            if (slowly && i % 2 == 0)
            {
                await Task.Delay(1);
            }
        }
        return rooms;
    }

    public async Task Separate(List<RoomVisualizer> rooms)
    {
        foreach (var room in rooms)
        {
            room.SetCollidesWithOtherRooms(true);
        }

        bool success = await WaitUntilAllBodiesSleep(rooms, 5.0f);
        GD.Print("Finished Success: ", success);

        foreach (var r in rooms)
        {
            r.SetCollidesWithOtherRooms(false);
        }
        await Task.Delay(1 * 1000);
    }

    public async Task Snap(List<RoomVisualizer> rooms)
    {
        // adjust all rooms to grid.
        foreach (var r in rooms)
        {
            r.SnapToGrid();
        }
        GD.Print("Done snapping to grid");
    }

    public async Task<List<RoomVisualizer>> SelectRooms(List<RoomVisualizer> rooms)
    {
        Vector2 meanSize = new Vector2();
        foreach (var room in rooms)
        {
            meanSize += room.GetSize();
        }
        meanSize = meanSize / rooms.Count();

        List<RoomVisualizer> selectedRooms = new List<RoomVisualizer>();
        float factor = 1.25f;
        foreach (var room in rooms)
        {
            var size = room.GetSize();
            if (size.X > meanSize.X * factor && size.Y > meanSize.Y * factor)
            {
                // select it!
                selectedRooms.Add(room);
                room.FillColor = new Color(0, 1, 0);
                await Task.Delay(100);
            }
        }
        return selectedRooms;
    }

    public async Task<List<(RoomVisualizer roomA, RoomVisualizer roomB)>> RelateSelectedRooms(
        List<RoomVisualizer> selectedRooms
    )
    {
        Dictionary<Vector2, int> centroidToRoomIndex = new Dictionary<Vector2, int>();
        for (int i = 0; i < selectedRooms.Count; i++)
        {
            centroidToRoomIndex[selectedRooms[i].GetCentroid()] = i;
        }
        List<Vector2> centroids = centroidToRoomIndex.Keys.ToList();

        var triangulation = new DelaunayTriangulation(centroids);
        var triangles = triangulation.Triangulate();
        var ddTriangles = new DebugDrawer();
        AddChild(ddTriangles);

        foreach (var t in triangles)
        {
            ddTriangles.AddLine(t.A, t.B, new Color(1, 0, 0));
            ddTriangles.AddLine(t.B, t.C, new Color(1, 0, 0));
            ddTriangles.AddLine(t.C, t.A, new Color(1, 0, 0));
        }
        await Task.Delay(1 * 1000);
        ddTriangles.QueueFree();

        // link a graph of rooms,
        var graph = new Graph<Vector2>();
        foreach (var t in triangles)
        {
            graph.AddBidirectional(t.A, t.B, t.A.DistanceSquaredTo(t.B));
            graph.AddBidirectional(t.B, t.C, t.B.DistanceSquaredTo(t.C));
            graph.AddBidirectional(t.C, t.A, t.C.DistanceSquaredTo(t.A));
        }

        // Compute MST
        var mst = graph.PrimMST();
        GD.Print($"MST done");

        var list = new List<(RoomVisualizer roomA, RoomVisualizer roomB)>();
        foreach (var edge in mst)
        {
            list.Add(
                (
                    selectedRooms[centroidToRoomIndex[edge.A]],
                    selectedRooms[centroidToRoomIndex[edge.B]]
                )
            );
        }
        var ddMst = new DebugDrawer();
        AddChild(ddMst);
        foreach (var edge in mst)
        {
            ddMst.AddLine(edge.A, edge.B, new Color(1, 0, 0));
        }
        await Task.Delay(1 * 1000);
        ddMst.QueueFree();
        return list;
    }

    public async Task<List<(Vector2 A, Vector2 B)>> AddHallways(
        List<(RoomVisualizer roomA, RoomVisualizer roomB)> roomEdges
    )
    {
        var hallways = new List<(Vector2 A, Vector2 B)>();
        foreach (var (roomA, roomB) in roomEdges)
        {
            var roomARect = roomA.GetRect();
            var roomBRect = roomB.GetRect();

            var roomACenter = roomARect.GetCenter();
            var roomBCenter = roomBRect.GetCenter();

            var midpoint = (roomACenter - roomBCenter) / 2 + roomBCenter;
            // horizontal hallway
            if (
                MathUtils.IsBetween(
                    midpoint.Y,
                    roomARect.Position.Y,
                    roomARect.Position.Y + roomARect.Size.Y
                )
                && MathUtils.IsBetween(
                    midpoint.Y,
                    roomBRect.Position.Y,
                    roomBRect.Position.Y + roomBRect.Size.Y
                )
            )
            {
                var hallwayStart = new Vector2(roomACenter.X, midpoint.Y).SnapToGrid();
                var hallwayEnd = new Vector2(roomBCenter.X, midpoint.Y).SnapToGrid();
                hallways.Add((hallwayStart, hallwayEnd));
            }
            // vertical hallway
            else if (
                MathUtils.IsBetween(
                    midpoint.X,
                    roomARect.Position.X,
                    roomARect.Position.X + roomARect.Size.X
                )
                && MathUtils.IsBetween(
                    midpoint.X,
                    roomBRect.Position.X,
                    roomBRect.Position.X + roomBRect.Size.X
                )
            )
            {
                var hallwayStart = new Vector2(midpoint.X, roomACenter.Y).SnapToGrid();
                var hallwayEnd = new Vector2(midpoint.X, roomBCenter.Y).SnapToGrid();
                hallways.Add((hallwayStart, hallwayEnd));
            }
            // both!
            else
            {
                var sourceStart = roomACenter.SnapToGrid();
                var sourceEnd = new Vector2(roomACenter.X, roomBCenter.Y).SnapToGrid();
                var targetEnd = roomBCenter.SnapToGrid();

                hallways.Add((sourceStart, sourceEnd));
                hallways.Add((sourceEnd, targetEnd));
            }
        }

        var ddHalways = new DebugDrawer();
        ddHalways.Thickness = 5f;
        var hallwayColor = new Color(0, 0, 0);
        AddChild(ddHalways);
        // debuggin
        foreach (var (prev, next) in hallways)
        {
            ddHalways.AddLine(prev, next, hallwayColor);
        }
        return hallways;
    }

    public async Task IntersectRooms(List<(Vector2 A, Vector2 B)> hallways)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        foreach (var (A, _B) in hallways)
        {
            var isVertical = A.X == _B.X;
            var B = _B;

            // push B out to thicken to hallway size, so it's not a hallway with a 0 dimension.
            if (isVertical)
            {
                B += new Vector2(Dim, 0);
            }
            else
            {
                B += new Vector2(0, Dim);
            }

            var BA = B - A;
            var rectCenter = BA / 2 + A;
            var size = BA.Abs();

            var rectangle = new RectangleShape2D();
            rectangle.Size = size;

            var queryParameters = new PhysicsShapeQueryParameters2D
            {
                Shape = rectangle,
                Transform = new Transform2D(0, rectCenter),
                CollisionMask = CollisionLayers.ROOM,
            };
            var results = spaceState.IntersectShape(queryParameters, 150);

            var interceptVis = new DebugRectangle();
            interceptVis.FillColor = new Color(0, 0, 0, 0);
            interceptVis.BorderColor = new Color(1, 0, 0);
            interceptVis.Size = size;
            interceptVis.Position = rectCenter - size / 2;
            AddChild(interceptVis);

            foreach (var result in results)
            {
                var room = result["collider"].As<RoomVisualizer>();
                if (room == null)
                    continue;

                room.FillColor = new Color(0, 1, 0);
            }
            await Task.Delay(2 * 1000);
        }

        GD.Print("done intersect rooms!");
    }

    private Vector2 GenerateNormalRadomRoomSize()
    {
        float width = Mathf.Max(
            1,
            Mathf.Round(MathUtils.NormalDistribution(meanWidth, stddevWidth, rng))
        );
        float height = Mathf.Max(
            1,
            Mathf.Round(MathUtils.NormalDistribution(meanHeight, stddevHeight, rng))
        );
        return new Vector2(width, height);
    }

    public Task<bool> WaitUntilAllBodiesSleep<T>(List<T> bodies, float timeoutSeconds)
        where T : RigidBody2D
    {
        var tcs = new TaskCompletionSource<bool>();
        _ = CheckBodiesSleepingAsync(bodies, timeoutSeconds, tcs);
        return tcs.Task;
    }

    private async Task CheckBodiesSleepingAsync<T>(
        List<T> bodies,
        float timeoutSeconds,
        TaskCompletionSource<bool> tcs
    )
        where T : RigidBody2D
    {
        float elapsed = 0f;
        float checkInterval = 0.05f; // Check every 50ms

        while (elapsed < timeoutSeconds)
        {
            if (bodies.All(body => body.Sleeping))
            {
                tcs.TrySetResult(true);
                return;
            }

            // Wait for the next check
            await Task.Delay((int)(checkInterval * 1000));
            elapsed += checkInterval;
        }

        // Timeout reached, complete the task as false
        tcs.TrySetResult(false);
    }
}
