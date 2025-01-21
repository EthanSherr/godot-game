using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class RoomGenerator : Node2D
{
    // ???
    [Export]
    private string tileMapperScenePath = "res://scripts/generation/TileMapper.tscn";

    private TileMapper tileMapper;

    // normal random
    float meanWidth = 13f;
    float stddevWidth = 4f;
    float meanHeight = 10f;
    float stddevHeight = 5f;

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

        PackedScene tileLayerPackedScene = GD.Load<PackedScene>(tileMapperScenePath);
        if (tileLayerPackedScene == null)
        {
            GD.Print($"Error loading packed scene {tileMapperScenePath}");
            return;
        }
        tileMapper = (TileMapper)tileLayerPackedScene.Instantiate();
        AddChild(tileMapper);

        GenerateDungeon();
    }

    public async void GenerateDungeon()
    {
        AddChild(makeGrid());

        var rooms = await GenerateRooms();
        await Separate(rooms);
        await Snap(rooms);

        var selectedRooms = await SelectRooms(rooms);
        var edges = await RelateSelectedRooms(selectedRooms);
        var hallways = await AddHallways(edges);
        var rooms2 = await IntersectRooms(hallways);

        await FillRooms(rooms2, hallways);
        SpawnPlayer(selectedRooms);
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
            Vector2I dimension = GenerateNormalRadomRoomSize();
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
        await Task.Delay(200);
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
                await Task.Delay(1);
            }
        }
        return selectedRooms;
    }

    // make a MST graph of selected rooms after triangulation
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
        await Task.Delay(200);
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
        await Task.Delay(200);
        ddMst.QueueFree();
        return list;
    }

    public async Task<List<Hallway>> AddHallways(
        List<(RoomVisualizer roomA, RoomVisualizer roomB)> roomEdges
    )
    {
        var hallways = new List<Hallway>();
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
                hallways.Add(new Hallway(hallwayStart, hallwayEnd));
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
                hallways.Add(new Hallway(hallwayStart, hallwayEnd));
            }
            // both!
            else
            {
                var sourceStart = roomACenter.SnapToGrid();
                var sourceEnd = new Vector2(roomACenter.X, roomBCenter.Y).SnapToGrid();
                var targetEnd = roomBCenter.SnapToGrid();

                hallways.Add(new Hallway(sourceStart, sourceEnd));
                hallways.Add(new Hallway(sourceEnd, targetEnd));
            }
        }

        var ddHalways = new DebugDrawer();
        ddHalways.Thickness = 5f;
        var hallwayColor = new Color(0, 0, 0);
        AddChild(ddHalways);
        // debuggin
        foreach (var h in hallways)
        {
            ddHalways.AddLine(h.A, h.B, hallwayColor);
        }
        return hallways;
    }

    public async Task<List<RoomVisualizer>> IntersectRooms(List<Hallway> hallways)
    {
        var intersectedRooms = new List<RoomVisualizer>();
        var debugNodes = new List<Node2D>();
        var spaceState = GetWorld2D().DirectSpaceState;
        foreach (var h in hallways)
        {
            var A = h.A;
            var _B = h.B;
            var B = _B;

            // push B out to thicken to hallway size, so it's not a hallway with a 0 dimension.
            if (h.Orientation == Hallway.OrientationType.Vertical)
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
            // var rect = h.GetRect();
            // var size = rect.Size;
            // var rectCenter = rect.GetCenter();

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
            interceptVis.FillColor = new Color(0.2f, 0, 0, 0.2f);
            interceptVis.BorderColor = new Color(1f, 0, 0);
            interceptVis.Size = size;
            interceptVis.Position = rectCenter - size / 2;
            AddChild(interceptVis);
            debugNodes.Add(interceptVis);

            foreach (var result in results)
            {
                var room = result["collider"].As<RoomVisualizer>();
                if (room == null)
                    continue;

                room.FillColor = new Color(0, 1, 0, 0.2f);
                intersectedRooms.Add(room);
            }
            await Task.Delay(10);
        }

        await Task.Delay(200);
        // foreach (var dbgNodes in debugNodes)
        // {
        //     dbgNodes.QueueFree();
        // }

        return intersectedRooms;
    }

    public async Task FillRooms(List<RoomVisualizer> rooms, List<Hallway> hallways)
    {
        tileMapper.ZIndex = 1000;
        foreach (var r in rooms)
        {
            foreach (Vector2I position in r.GetPerimeter())
            {
                tileMapper.FillCell(position);
            }
        }
        await Task.Delay(200);

        // step along the hallway and remove blocks!
        foreach (var h in hallways)
        {
            var a = h.A;
            var b = h.B;
            var direction = (b - a) / Constants.GridSize;
            var unitDirection = direction.Normalized().Round();
            var iterations = (int)Mathf.Max(Mathf.Abs(direction.X), Mathf.Abs(direction.Y));
            var position = a;

            GD.Print($"hallway1 {a}-{b} has {iterations}");

            var lols = 2;
            for (var i = -lols; i < iterations + lols; i++)
            {
                position += unitDirection * Constants.GridSize;
                var grid = position.ToGrid();
                GD.Print($"hallway2 {grid}");
                tileMapper.UnfillCell(grid);
                // move to more convenient, ladders in all vertical hallways
                if (h.Orientation == Hallway.OrientationType.Vertical) {
                    tileMapper.AddLadder(grid);
                }
            }
        }

        // foreach (var hallway in hallways) {
        //     if (hallway.Orientation != Hallway.OrientationType.Vertical) {
        //         continue;
        //     }
        //     foreach (var cell in hallway.)
        // }
    }

    public void SpawnPlayer(List<RoomVisualizer> rooms)
    {
        var randIndx = rng.RandiRange(0, rooms.Count - 1);
        var room = rooms[randIndx];
        var rect = room.GetRect();
        var location =
            rect.Position
            + new Vector2(
                (rect.Size.X - 2 * Constants.GridSize) * rng.Randf(),
                (rect.Size.Y - 2 * Constants.GridSize) * rng.Randf()
            )
            + new Vector2(Constants.GridSize, Constants.GridSize);
        var snappedLocation = location.SnapToGrid();

        var spawn = PlayerSpawn.Create();
        spawn.Position = snappedLocation;
        AddChild(spawn);
        var dc = new DebugCircle();
        dc.Radius = 50;
        dc.Position = snappedLocation;
        AddChild(dc);

        var player = Player.Create();
        player.Position = spawn.Position + new Vector2(Constants.GridSize / 2, Constants.GridSize);
        player.ZIndex = 10000;
        AddChild(player);
        
        player.Initialize();
        GameManager.Instance.PossessCharacter(player);
    }

    private Vector2I GenerateNormalRadomRoomSize()
    {
        int width = Mathf.Max(
            1,
            (int)Mathf.Floor(MathUtils.NormalDistribution(meanWidth, stddevWidth, rng))
        );
        int height = Mathf.Max(
            1,
            (int)Mathf.Floor(MathUtils.NormalDistribution(meanHeight, stddevHeight, rng))
        );
        return new Vector2I(width, height);
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
