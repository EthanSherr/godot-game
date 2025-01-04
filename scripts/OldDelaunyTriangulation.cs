using System;
using System.Collections.Generic;
using Godot;

public class OldDelaunayTriangulation
{
    public class Triangle
    {
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsPointInCircumcircle(Vector2 p)
        {
            // Use determinant-based circumcircle test
            float ax = A.X - p.X;
            float ay = A.Y - p.Y;
            float bx = B.X - p.X;
            float by = B.Y - p.Y;
            float cx = C.X - p.X;
            float cy = C.Y - p.Y;

            float det =
                (ax * ax + ay * ay) * (bx * cy - by * cx)
                - (bx * bx + by * by) * (ax * cy - ay * cx)
                + (cx * cx + cy * cy) * (ax * by - ay * bx);

            return det > 0; // If > 0, point is inside circumcircle
        }
    }

    public List<Triangle> Triangulate(List<Vector2> points)
    {
        // Create a super-triangle large enough to contain all points
        Vector2 pMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 pMax = new Vector2(float.MinValue, float.MinValue);

        foreach (var p in points)
        {
            pMin = new Vector2(Mathf.Min(pMin.X, p.X), Mathf.Min(pMin.Y, p.Y));
            pMax = new Vector2(Mathf.Max(pMax.X, p.X), Mathf.Max(pMax.Y, p.Y));
        }

        float margin = 10f;
        Vector2 d = pMax - pMin;
        Vector2 center = pMin + d / 2;
        float radius = Mathf.Max(d.X, d.Y) * 2;

        Vector2 p1 = center + new Vector2(-radius, -radius);
        Vector2 p2 = center + new Vector2(radius, -radius);
        Vector2 p3 = center + new Vector2(0, radius);

        var superTriangle = new Triangle(p1, p2, p3);

        var triangles = new List<Triangle> { superTriangle };

        // Add each point to the triangulation
        foreach (var point in points)
        {
            var badTriangles = new List<Triangle>();

            // Find all triangles whose circumcircle contains the point
            foreach (var triangle in triangles)
            {
                if (triangle.ContainsPointInCircumcircle(point))
                {
                    badTriangles.Add(triangle);
                }
            }

            // Create a polygonal hole by removing bad triangles
            var edges = new List<(Vector2, Vector2)>();
            foreach (var triangle in badTriangles)
            {
                edges.Add((triangle.A, triangle.B));
                edges.Add((triangle.B, triangle.C));
                edges.Add((triangle.C, triangle.A));
            }

            // Remove duplicate edges (shared edges)
            edges = RemoveDuplicateEdges(edges);

            // Remove bad triangles
            triangles.RemoveAll(t => badTriangles.Contains(t));

            // Add new triangles connecting point to polygon edges
            foreach (var edge in edges)
            {
                triangles.Add(new Triangle(edge.Item1, edge.Item2, point));
            }
        }

        // Remove triangles connected to the super-triangle
        triangles.RemoveAll(t =>
            t.A == p1
            || t.A == p2
            || t.A == p3
            || t.B == p1
            || t.B == p2
            || t.B == p3
            || t.C == p1
            || t.C == p2
            || t.C == p3
        );

        return triangles;
    }

    private List<(Vector2, Vector2)> RemoveDuplicateEdges(List<(Vector2, Vector2)> edges)
    {
        var uniqueEdges = new List<(Vector2, Vector2)>();
        foreach (var edge in edges)
        {
            var reversed = (edge.Item2, edge.Item1);
            if (!uniqueEdges.Contains(edge) && !uniqueEdges.Contains(reversed))
            {
                uniqueEdges.Add(edge);
            }
        }
        return uniqueEdges;
    }
}
