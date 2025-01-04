using System;
using System.Collections.Generic;
using Godot;

public struct Triangle
{
    public Vector2 A;
    public Vector2 B;
    public Vector2 C;

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
}

public class DelaunayTriangulation
{
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

    public List<int> Triangulate(List<Vector2> centroids)
    {
        var superTriangle = MakeSuperTriangle(centroids);
        var triangles = new List<Triangle> { superTriangle };

        GD.Print("centroids", centroids.Count);
        GD.Print("triangle", triangles.Count);
        foreach (var centroid in centroids)
        {
            foreach (var triangle in triangles)
            {
                if (triangle.ContainsPointInCircumcircle(centroid))
                {
                    GD.Print($"centroid {centroid} is in triangle centercircle!");
                }
            }
        }

        return new List<int>();
    }
}
