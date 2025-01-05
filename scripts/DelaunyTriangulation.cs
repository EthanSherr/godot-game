using System;
using System.Collections.Generic;
using Godot;

public struct Edge
{
    public readonly Vector2 A; // Readonly field
    public readonly Vector2 B; // Readonly field

    public Edge(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
    }

    // Override Equals to define equality for edges
    public override bool Equals(object? obj)
    {
        if (obj is Edge other)
        {
            // Treat the edge as undirected: A->B is the same as B->A
            return (A == other.A && B == other.B) || (A == other.B && B == other.A);
        }
        return false;
    }

    // Override GetHashCode to handle undirected equality
    public override int GetHashCode()
    {
        // Order the points to ensure consistent hashing (smallest first)
        var hashA = A.GetHashCode();
        var hashB = B.GetHashCode();
        return HashCode.Combine(Math.Min(hashA, hashB), Math.Max(hashA, hashB));
    }

    public override string ToString()
    {
        return $"Edge({A}, {B})";
    }
}

public struct Triangle
{
    public Vector2 A;
    public Vector2 B;
    public Vector2 C;

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
        _circumCircle = null;
    }

    private (Vector2 Center, float SquaredRadius)? _circumCircle;

    public (Vector2 Center, float SquaredRadius) CircumCircle
    {
        get
        {
            if (!_circumCircle.HasValue)
            {
                _circumCircle = MathUtils.CircumCircle(A, B, C);
            }
            return _circumCircle.Value;
        }
    }

    public bool ContainsPointInCircumcircle(Vector2 p)
    {
        var (center, squaredRadius) = CircumCircle;
        return p.DistanceSquaredTo(center) <= squaredRadius;
    }

    public override string ToString()
    {
        return $"Triangle({A}, {B}, {C})";
    }
}

public class DelaunayTriangulation
{
    public List<Vector2> Centroids;
    public Triangle SuperTriangle;
    public List<Triangle> Triangles;

    public DelaunayTriangulation(List<Vector2> centroids)
    {
        Centroids = centroids;
        SuperTriangle = MakeSuperTriangle(Centroids);
        Triangles = new List<Triangle> { SuperTriangle };
    }

    public void VisitVertex(int index)
    {
        var centroid = Centroids[index];
        var edges = new Dictionary<Edge, int>();

        for (int tIndex = 0; tIndex < Triangles.Count; tIndex++)
        {
            var triangle = Triangles[tIndex];
            if (triangle.ContainsPointInCircumcircle(centroid))
            {
                ListUtils.SwapAndPop(Triangles, tIndex--);
                var e1 = new Edge(triangle.A, triangle.B);
                var e2 = new Edge(triangle.B, triangle.C);
                var e3 = new Edge(triangle.C, triangle.A);

                edges[e1] = edges.GetValueOrDefault(e1, 0) + 1;
                edges[e2] = edges.GetValueOrDefault(e2, 0) + 1;
                edges[e3] = edges.GetValueOrDefault(e3, 0) + 1;
            }
        }

        foreach (var (edge, count) in edges)
        {
            if (count > 1)
            {
                continue;
            }
            var newTriangle = new Triangle(edge.A, edge.B, centroid);
            Triangles.Add(newTriangle);
        }
    }

    public void RemoveEdgesInSuperTriangle()
    {
        for (int tIndex = 0; tIndex < Triangles.Count; tIndex++)
        {
            var triangle = Triangles[tIndex];
            if (
                triangle.A == SuperTriangle.A
                || triangle.A == SuperTriangle.B
                || triangle.A == SuperTriangle.C
                || triangle.B == SuperTriangle.A
                || triangle.B == SuperTriangle.B
                || triangle.B == SuperTriangle.C
                || triangle.C == SuperTriangle.A
                || triangle.C == SuperTriangle.B
                || triangle.C == SuperTriangle.C
            )
            {
                ListUtils.SwapAndPop(Triangles, tIndex--);
            }
        }
    }

    // https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/#the-super-triangle
    public Triangle MakeSuperTriangle(List<Vector2> centroids)
    {
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        foreach (var centroid in centroids)
        {
            minX = Math.Min(centroid.X, minX);
            minY = Math.Min(centroid.Y, minY);
            maxX = Math.Max(centroid.X, maxX);
            maxY = Math.Max(centroid.Y, maxY);
        }

        var dx = (maxX - minX) * 10;
        var dy = (maxY - minY) * 10;

        return new Triangle
        {
            A = new Vector2(minX - dx, minY - dy * 3),
            B = new Vector2(minX - dx, maxY + dy),
            C = new Vector2(maxX + dx * 3, maxY + dy),
        };
    }

    public List<Triangle> Triangulate()
    {
        for (var centroidIndex = 0; centroidIndex < Centroids.Count; centroidIndex++)
        {
            VisitVertex(centroidIndex);
        }

        RemoveEdgesInSuperTriangle();

        return Triangles;
    }
}
