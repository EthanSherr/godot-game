using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Triangulation : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // testHashSetEdge();
        DoRealTest();
    }

    public void testHashSetEdge()
    {
        var p1 = new Vector2(0.123f, 1234.2123f);
        var p2 = new Vector2(0.5653423f, -21323.222123f);
        var edge1 = new Edge(p1, p2);
        var edge2 = new Edge(p2, p1);

        var set = new HashSet<Edge>();
        set.Add(edge1);
        set.Add(edge2);

        if (!edge1.Equals(edge2))
        {
            GD.Print("edge1 doesnt equal edge2!");
        }

        GD.Print(set.Count);

        GD.Print("set contains this edge ?", set.Contains(new Edge(p1, p2)));
    }

    public async void DoRealTest()
    {
        var tempList = new List<int> { 1, 2, 3, 4, 5 };
        // ListUtils.SwapAndPop(tempList, 0);

        for (var i = 0; i < tempList.Count; i++)
        {
            if (tempList[i] % 2 == 0)
            {
                ListUtils.SwapAndPop(tempList, i--);
            }
        }
        GD.Print("remove list", string.Join(", ", tempList));

        var centroids = NodeUtils
            .FindAllNodesOfType<Marker2D>(this)
            .Select(marker => marker.Position)
            .ToList();

        foreach (var c in centroids)
        {
            AddChild(
                new DebugCircle
                {
                    Radius = 8.0f,
                    Position = c,
                    FillColor = null, //new Color(0, 0, 1),
                    LineColor = new Color(1, 0, 0),
                    LineWidth = 3f,
                }
            );
        }

        var dd = new DebugDrawer();
        AddChild(dd);

        var delany = new DelaunayTriangulation(centroids);

        var lastDebugObjects = new List<Node2D>();

        for (var i = 0; i < centroids.Count; i++)
        {
            lastDebugObjects.ForEach(d => d.QueueFree());
            lastDebugObjects.Clear();

            delany.VisitVertex(i);

            lastDebugObjects.Add(
                new DebugCircle
                {
                    FillColor = new Color(0, 1, 0),
                    Radius = 5,
                    Position = centroids[i],
                }
            );

            // after I add a new vertex, I should only see one circumcircle passing through the piont
            // var tNoSuper = delany.Triangles.Filter
            var overlapCount = 0;
            var nonOverlapCount = 0;
            foreach (var triangle in delany.Triangles)
            {
                var isOverlapping = triangle.ContainsPointInCircumcircle(centroids[i]);

                if (isOverlapping)
                {
                    overlapCount++;
                }
                else
                {
                    nonOverlapCount++;
                }

                var color = isOverlapping ? new Color(1, 0, 0) : new Color(0, 1, 0);
                lastDebugObjects.Add(
                    new DebugCircle
                    {
                        LineColor = color,
                        LineWidth = 1,
                        FillColor = null,
                        Radius = (float)Math.Sqrt(triangle.CircumCircle.SquaredRadius),
                        Position = triangle.CircumCircle.Center,
                    }
                );
            }

            lastDebugObjects.ForEach(d => AddChild(d));

            await Task.Delay(2 * 1000);
        }

        lastDebugObjects.ForEach(d => d.QueueFree());

        delany.RemoveEdgesInSuperTriangle();

        foreach (var t in delany.Triangles)
        {
            var d = new DebugDrawer();
            d.AddTriangle(t.A, t.B, t.C, new Color(0, 0, 1));
            lastDebugObjects.Add(d);
        }

        lastDebugObjects.ForEach(d => AddChild(d));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
