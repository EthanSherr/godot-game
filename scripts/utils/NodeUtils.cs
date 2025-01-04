using System.Collections.Generic;
using Godot;

public static class NodeUtils
{
    public static List<T> FindAllNodesOfType<T>(Node parent)
        where T : Node
    {
        List<T> nodesOfType = new List<T>();

        foreach (Node child in parent.GetChildren())
        {
            if (child is T node)
            {
                nodesOfType.Add(node);
            }

            // Recursively search the child's children
            nodesOfType.AddRange(FindAllNodesOfType<T>(child));
        }

        return nodesOfType;
    }
}
