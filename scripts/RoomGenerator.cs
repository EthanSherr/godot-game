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
            block.Position = RandomPointInCircle(Radius);
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

        var debugDraw = new DebugDrawer();
        AddChild(debugDraw);

        foreach (var triangle in triangles)
        {
            var centroidA = triangle.A;
            var centroidB = triangle.B;
            var centroidC = triangle.C;

            var roomA = rooms[centroidToRoomIndex[triangle.A]];
            var roomB = rooms[centroidToRoomIndex[triangle.B]];
            var roomC = rooms[centroidToRoomIndex[triangle.C]];

            debugDraw.AddLine(centroidA, centroidB, new Color(1, 0, 0));
            debugDraw.AddLine(centroidB, centroidC, new Color(1, 0, 0));
            debugDraw.AddLine(centroidC, centroidA, new Color(1, 0, 0));
        }
    }

    public Vector2 RandomPointInCircle(float radius)
    {
        float theta = 2 * (float)Math.PI * rng.Randf();
        float u = rng.Randf() + rng.Randf();
        float r = u > 1 ? 2 - u : u;
        return new Vector2(
            radius * r * (float)Math.Cos(theta),
            radius * r * (float)Math.Sin(theta)
        );
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
        float width = Mathf.Max(1, Mathf.Round(NormalDistribution(meanWidth, stddevWidth)));
        float height = Mathf.Max(1, Mathf.Round(NormalDistribution(meanHeight, stddevHeight)));
        return new Vector2(width, height);
    }

    // USEFUL, STANDARDIZE?
    private float NormalDistribution(float mean, float stddev)
    {
        // Box-Muller transform
        float u1 = rng.Randf();
        float u2 = rng.Randf();
        float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.Pi * u2);
        return mean + z * stddev;
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
