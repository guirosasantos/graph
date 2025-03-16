namespace Lib;

public sealed class Node(int id)
{
    public int Id { get; init; } = id;
    public List<Edge> Edges { get; private set; } = [];
}