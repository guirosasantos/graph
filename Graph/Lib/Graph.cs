using Lib.Enuns;
using Lib.RepresentationTypes;

namespace Lib;

public sealed class Graph(bool isDirected, bool isWeighted, GraphType graphType)
{
    public List<AdjacentListNode> AdjacentList { get; private set; } = [];
    //public List<AdjacentMatrixNode> AdjacentMatrix { get; private set; } = [];
    public bool IsDirected { get; private init; } = isDirected;
    public bool IsWeighted { get; private init; } = isWeighted;
    public GraphType GraphType { get; private init; } = graphType;

    public bool InsertNode(string label)
    {
        if (GraphAlreadyHasNode(label))
            return false;

        var node = new Node(label);

        return AddNodeToGraph(node);
    }

    private bool GraphAlreadyHasNode(string label)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Any(node => node.IndexNode.Label == label),
            GraphType.Matrix => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    private bool GraphAlreadyHasNode(int index, out string label)
    {
        label = string.Empty;

        var exists = GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index] != null,
            GraphType.Matrix => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        if (exists)
            label = AdjacentList[index].IndexNode.Label;

        return exists;
    }

    private bool AddNodeToGraph(Node node)
    {
        switch (GraphType)
        {
            case GraphType.AdjacentList:
                AdjacentList.Add(new AdjacentListNode(node, []));
                break;
            case GraphType.Matrix:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
        }

        return true;
    }

    public bool RemoveNode(int index)
    {
        if (!GraphAlreadyHasNode(index, out var label))
            return false;

        switch (GraphType)
        {
            case GraphType.AdjacentList:
                RemoveNodeFromAdjacentList(label);
                break;
            case GraphType.Matrix:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
        }

        return true;
    }

    private void RemoveNodeFromAdjacentList(string label)
    {
        RemoveNodeFromAdjacentListEdges(label);
        RemoveNodeFromIndex(label);
    }

    private void RemoveNodeFromAdjacentListEdges(string label)
    {
        AdjacentList.ForEach(node =>
        {
            node.AdjacentNodes.RemoveAll(adjacentNode => adjacentNode.Label == label);
            node.IndexNode.Edges.RemoveAll(edge => edge.To.Label == label);
        });
    }

    private void RemoveNodeFromIndex(string label)
    {
        AdjacentList.RemoveAll(node => node.IndexNode.Label == label);
    }

    public string LabelNode(int index)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index].IndexNode.Label,
            GraphType.Matrix => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    public void PrintGraph()
    {
        Console.WriteLine("index | label | list of adjacent nodes");

        switch (GraphType)
        {
            case GraphType.AdjacentList:
                PrintAdjacentList();
                break;
            case GraphType.Matrix:
                throw new NotImplementedException("Matrix graph printing not implemented yet");
            default:
                throw new NotImplementedException("Unknown graph type");
        }
    }

    private void PrintAdjacentList()
    {
        for (int i = 0; i < AdjacentList.Count; i++)
        {
            var node = AdjacentList[i];
            var adjacentLabels = string.Join(", ", node.AdjacentNodes.Select(n => n.Label));

            Console.WriteLine($"{i} - {node.IndexNode.Label} - [{adjacentLabels}]");
        }
    }

    public bool AddEdge(int origin, int destination, int weight = 1)
    {
        if (IsWeighted && weight == 0)
            return false;

        if (!IsWeighted && weight != 1)
            return false;

        if (!GraphAlreadyHasNode(origin, out _) || !GraphAlreadyHasNode(destination, out _))
            return false;

        var originNode = GetNode(origin);
        var destinationNode = GetNode(destination);

        var addEdgesResult = AddEdgeToGraph(originNode, destinationNode, weight);

        if (!addEdgesResult)
            return false;

        return AddToAdjacentList(origin, destination);
    }

    private Node GetNode(int index)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index].IndexNode,
            GraphType.Matrix => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    private bool AddEdgeToGraph(Node originNode, Node destinationNode, int weight)
        => IsDirected
            ? originNode.AddEdge(destinationNode, weight)
            : originNode.AddEdge(destinationNode, weight) && destinationNode.AddEdge(originNode, weight);

    private bool AddToAdjacentList(int origin, int destination)
    {
        if (IsDirected)
        {
            AdjacentList[origin].AdjacentNodes.Add(AdjacentList[destination].IndexNode);
            return true;
        }

        AdjacentList[origin].AdjacentNodes.Add(AdjacentList[destination].IndexNode);
        AdjacentList[destination].AdjacentNodes.Add(AdjacentList[origin].IndexNode);

        return true;
    }

    public bool RemoveEdge(int origin, int destination)
    {
        if (!GraphAlreadyHasNode(origin, out _) || !GraphAlreadyHasNode(destination, out _))
            return false;

        var originNode = GetNode(origin);
        var destinationNode = GetNode(destination);

        var removeEdgesResult = RemoveEdgeFromGraph(originNode, destinationNode);

        if (!removeEdgesResult)
            return false;

        return RemoveFromAdjacentList(origin, destination);
    }

    private bool RemoveEdgeFromGraph(Node originNode, Node destinationNode)
        => IsDirected
            ? originNode.RemoveEdge(destinationNode)
            : originNode.RemoveEdge(destinationNode) && destinationNode.RemoveEdge(originNode);

    private bool RemoveFromAdjacentList(int origin, int destination)
    {
        if (IsDirected)
        {
            AdjacentList[origin].AdjacentNodes.RemoveAll(node => node.Label == AdjacentList[destination].IndexNode.Label);
            return true;
        }

        AdjacentList[origin].AdjacentNodes.RemoveAll(node => node.Label == AdjacentList[destination].IndexNode.Label);
        AdjacentList[destination].AdjacentNodes.RemoveAll(node => node.Label == AdjacentList[origin].IndexNode.Label);

        return true;
    }

    public bool DoesEdgeExist(int origin, int destination)
    {
        if (!GraphAlreadyHasNode(origin, out _) || !GraphAlreadyHasNode(destination, out _))
            return false;

        return GetNode(origin).Edges.Any(edge => edge.To.Label == LabelNode(destination));
    }

    public int GetEdgeWeight(int origin, int destination)
    {
        if (!GraphAlreadyHasNode(origin, out _) || !GraphAlreadyHasNode(destination, out _))
            return -1;

        return GetNode(origin).Edges.First(edge => edge.To.Label == LabelNode(destination)).Weight;
    }

    public List<Node> GetAdjacentNodes(int index)
    {
        if (!GraphAlreadyHasNode(index, out _))
            return [];

        return AdjacentList[index].AdjacentNodes;
    }
}