namespace Lib;

public sealed class ListGraph(bool isDirected, bool isWeighted) : Graph(isDirected, isWeighted)
{
    public List<AdjacentListNode> AdjacentList { get; private set; } = [];
}