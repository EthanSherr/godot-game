using System.Collections.Generic;
using System.Linq;
using Godot;

public class Graph<T>
{
    public struct Edge<T>
    {
        public readonly T A;

        public readonly T B;

        public readonly float W;

        public Edge(T a, T b, float w)
        {
            A = a;
            B = b;
            W = w;
        }
    }

    public Dictionary<T, Dictionary<T, float>> map;

    public Graph()
    {
        map = new Dictionary<T, Dictionary<T, float>>();
    }

    public bool Contains(T a)
    {
        return map.ContainsKey(a);
    }

    public void Add(T a, T b, float w)
    {
        var edges = getOrInitializeEdges(a);
        edges[b] = w;
        Add(b);
    }

    public void AddBidirectional(T a, T b, float w)
    {
        Add(a, b, w);
        Add(b, a, w);
    }

    public void Add(T a)
    {
        getOrInitializeEdges(a);
    }

    private Dictionary<T, float> getOrInitializeEdges(T a)
    {
        Dictionary<T, float> edges;
        if (!map.TryGetValue(a, out edges))
        {
            // GD.Print($"2 init new dict as edges");
            edges = new Dictionary<T, float>();
            map[a] = edges;
        }
        return edges;
    }

    public Graph<T> Clone()
    {
        var g = new Graph<T>();
        foreach (var (a, aEdges) in map)
        {
            foreach (var (b, w) in aEdges)
            {
                g.Add(a, b, w);
            }
        }
        return g;
    }

    public int Count()
    {
        return map.Count;
    }

    public List<Edge<T>> PrimMST()
    {
        var minSpanningTree = new List<Edge<T>>();
        var visited = new HashSet<T>();
        var minHeap = new PriorityQueue<Edge<T>, float>();

        var (startNode, startEdges) = map.ElementAt(0);
        visited.Add(startNode);
        foreach (var (b, w) in startEdges)
        {
            minHeap.Enqueue(new Edge<T>(startNode, b, w), w);
        }

        while (visited.Count < Count())
        {
            var edge = minHeap.Dequeue();
            if (visited.Contains(edge.B))
                continue;

            minSpanningTree.Add(edge);
            visited.Add(edge.B);

            foreach (var (c, w) in map[edge.B])
            {
                if (visited.Contains(c))
                    continue;
                minHeap.Enqueue(new Edge<T>(edge.B, c, w), w);
            }
        }

        return minSpanningTree;
    }
}
