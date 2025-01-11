using System;
using Godot;

public static class MathUtils
{
    public static Vector2 IntersectLines(Vector2 P1, Vector2 D1, Vector2 P2, Vector2 D2)
    {
        /*
            t1 * D1 + P1 = t2 * D2 + P2

            D1.X  -D2.X   | t1 = P2 - P1
            D1.Y  -D2.Y   | t2
          
            m11   m21 | c1
            m12   m22 | c2
        */

        float m11 = D1.X;
        float m21 = -D2.X;
        float m12 = D1.Y;
        float m22 = -D2.Y;

        float c1 = P2.X - P1.X;
        float c2 = P2.Y - P1.Y;

        // if pivots are 0 let's do a row swap!
        if (m11 == 0 || m22 == 0)
        {
            var m11Temp = m11;
            var m21Temp = m21;
            var c1Temp = c1;

            m11 = m12;
            m21 = m22;
            c1 = c2;

            m12 = m11Temp;
            m22 = m21Temp;
            c2 = c1Temp;
        }
        // first row reduction
        /*
            m11   m21 | c1 * (-m12/m11)
            m12   m22 | c2
        */
        if (m11 == 0)
        {
            // bail
        }
        var m12OverM11 = -(m12 / m11);
        m22 += m21 * m12OverM11;
        c2 += c1 * m12OverM11;
        // m12 += m11 * m12OverM11;
        m12 = 0;

        // second row reduction
        /*
            m11   m21 | c1
            0     m22 | c2 * (-m21/m22)
        */
        if (m22 == 0)
        {
            // bail
        }
        var m21OverM22 = -(m21 / m22);
        c1 += c2 * m21OverM22;
        // m21 += m22 * m21OverM22;
        // m21 = 0;

        // normalize first row
        c1 /= m11;
        // m11 /= m11... m12 /= m12 but m12 = 0 anwyay.

        return c1 * D1 + P1;
    }

    public static (Vector2 center, float squaredRadius) CircumCircle(
        Vector2 A,
        Vector2 B,
        Vector2 C
    )
    {
        var BMinusA = B - A;
        var CMinusA = C - A;

        var BAMid = BMinusA / 2 + A;
        var BAMidDir = new Vector2(BMinusA.Y, -BMinusA.X);
        var CAMid = CMinusA / 2 + A;
        var CAMidDir = new Vector2(CMinusA.Y, -CMinusA.X);

        var center = IntersectLines(BAMid, BAMidDir, CAMid, CAMidDir);

        float squaredRadius = center.DistanceSquaredTo(A);

        return (center, squaredRadius);
    }

    public static float NormalDistribution(float mean, float stddev, RandomNumberGenerator rng)
    {
        // Box-Muller transform
        float u1 = rng.Randf();
        float u2 = rng.Randf();
        float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.Pi * u2);
        return mean + z * stddev;
    }

    public static Vector2 RandomPointInCircle(float radius, RandomNumberGenerator rng)
    {
        float theta = 2 * (float)Math.PI * rng.Randf();
        float u = rng.Randf() + rng.Randf();
        float r = u > 1 ? 2 - u : u;
        return new Vector2(
            radius * r * (float)Math.Cos(theta),
            radius * r * (float)Math.Sin(theta)
        );
    }

    public static float Truncate(float n)
    {
        return n > 0 ? Mathf.Floor(n) : Mathf.Ceil(n);
    }

    public static bool IsBetween(float value, float min, float max)
    {
        return min < value && value < max;
    }
}
