namespace Lib;

public sealed class Node(string label)
{
    public string Label { get; init; } = label;
    public List<Edge> Edges { get; private set; } = [];

    public bool AddEdge(Node to, float weight)
    {
        if (Edges.Any(edge => edge.To == to))
            return false;

        Edges.Add(new Edge(this, to, weight));

        return true;
    }

    public bool RemoveEdge(Node to)
    {
        var edge = Edges.FirstOrDefault(edge => edge.To == to);

        if (edge is null)
            return false;

        Edges.Remove(edge);

        return true;
    }
}