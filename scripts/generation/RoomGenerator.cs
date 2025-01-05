using System;
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

    float Dim = 16;

    // random
    Vector2I MinDims = new Vector2I(5, 5);
    Vector2I MaxDims = new Vector2I(125, 100);

    float Radius = 300;

    [Export]
    private ulong Seed = 0;

    private RandomNumberGenerator rng;

    private List<RoomVisualizer> rooms = new List<RoomVisualizer>();

    private bool slowly = true;

    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Seed = Seed;

        GenerateRooms();
    }

    public async void GenerateRooms()
    {
        for (int i = 0; i < 150; i++)
        {
            // Vector2I dimension = GenerateRandomDims();
            Vector2 dimension = GenerateNormalRadomRoomSize() * Dim;
            RoomVisualizer block = new RoomVisualizer
            {
                Width = dimension.X,
                Height = dimension.Y,
                Dim = 1,
                BorderColor = new Color(1, 0, 0),
                FillColor = new Color(0, 0, 1),
                BorderThickness = 1,
            };
            block.Position = MathUtils.RandomPointInCircle(Radius, rng);
            AddChild(block);

            rooms.Add(block);
            if (slowly)
            {
                await Task.Delay(1);
            }
        }

        Vector2 meanSize = new Vector2();
        foreach (var room in rooms)
        {
            room.Activate();
            meanSize += room.GetSize();
        }
        meanSize = meanSize / rooms.Count();

        bool success = await WaitUntilAllBodiesSleep(rooms, 5.0f);
        GD.Print("Finished Success: ", success);

        List<RoomVisualizer> selectedRooms = new List<RoomVisualizer>();

        float factor = 1.25f;
        foreach (var room in rooms)
        {
            var size = room.GetSize();
            if (size.X > meanSize.X * factor && size.Y > meanSize.Y * factor)
            {
                // select it!
                selectedRooms.Add(room);
            }
        }
        GD.Print("selected rooms", selectedRooms.Count);

        Dictionary<Vector2, int> centroidToRoomIndex = new Dictionary<Vector2, int>();
        for (int i = 0; i < selectedRooms.Count; i++)
        {
            centroidToRoomIndex[selectedRooms[i].Position] = i;
        }
        List<Vector2> centroids = centroidToRoomIndex.Keys.ToList();

        var triangulation = new DelaunayTriangulation(centroids);
        var triangles = triangulation.Triangulate();
        var ddTriangles = new DebugDrawer();
        AddChild(ddTriangles);

        foreach (var triangle in triangles)
        {
            var centroidA = triangle.A;
            var centroidB = triangle.B;
            var centroidC = triangle.C;

            ddTriangles.AddLine(centroidA, centroidB, new Color(1, 0, 0));
            ddTriangles.AddLine(centroidB, centroidC, new Color(1, 0, 0));
            ddTriangles.AddLine(centroidC, centroidA, new Color(1, 0, 0));
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
        GD.Print("Adding mst", mst.Count);
        var ddMst = new DebugDrawer();
        AddChild(ddMst);
        foreach (var edge in mst)
        {
            ddMst.AddLine(edge.A, edge.B, new Color(1, 0, 0));
        }

        // Don't forget you can then map these centroids back to rooms
        // var roomA = rooms[centroidToRoomIndex[triangle.A]];
        // var roomB = rooms[centroidToRoomIndex[triangle.B]];
        // var roomC = rooms[centroidToRoomIndex[triangle.C]];
    }

    public Vector2I GenerateRandomDims()
    {
        return new Vector2I(
            rng.RandiRange(MinDims.X, MaxDims.X),
            rng.RandiRange(MinDims.Y, MaxDims.Y)
        );
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
