using Lib.Enuns;
using Lib.RepresentationTypes;

namespace Lib;

public sealed class Graph(bool isDirected, bool isWeighted, GraphType graphType)
{
    public List<AdjacentListNode> AdjacentList { get; private set; } = [];
    public List<AdjacentMatrixNode> AdjacentMatrix { get; private set; } = [];
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
            GraphType.Matrix => AdjacentMatrix.Any(node => node.OriginNode.Label == label),
            _ => throw new NotImplementedException()
        };
    }

    private bool GraphAlreadyHasNode(int index, out string label)
    {
        label = string.Empty;

        var exists = GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index] != null,
            GraphType.Matrix => AdjacentMatrix[index] != null,
            _ => throw new NotImplementedException()
        };

        if (exists)
        {
            if (GraphType == GraphType.Matrix)
                label = AdjacentMatrix[index].OriginNode.Label;
            else
                label = AdjacentList[index].IndexNode.Label;
        }

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
                AddNewNodeToMatrix(node);
                break;
            default:
                throw new NotImplementedException();
        }

        return true;
    }

    private void AddNewNodeToMatrix(Node node)
    {
        var edges = new List<AdjacentMatrixEdge>();

        for (int i = 0; i < AdjacentMatrix.Count; i++)
            edges.Add(new AdjacentMatrixEdge(AdjacentMatrix[i].OriginNode, false));

        AdjacentMatrix.Add(new AdjacentMatrixNode(node, edges));

        RecalculateMatrixEdges();
    }

    private void RecalculateMatrixEdges()
    {
        var lastAddedNode = AdjacentMatrix.LastOrDefault()?.OriginNode;

        if (lastAddedNode == null)
            return;

        AdjacentMatrix.ForEach(nodeRelation =>
        {
            nodeRelation.Edges.Add(new AdjacentMatrixEdge(lastAddedNode, false));
        });
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
                RemoveNodeFromAdjacentMatrix(label);
                break;
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
        => AdjacentList.RemoveAll(node => node.IndexNode.Label == label);

    private void RemoveNodeFromAdjacentMatrix(string label)
    {
        RemoveNodeFromAdjacentMatrixEdges(label);
        RemoveNodeFromMatrixIndex(label);
    }

    private void RemoveNodeFromAdjacentMatrixEdges(string label)
    {
        AdjacentMatrix.ForEach(node =>
        {
            node.Edges.RemoveAll(edge => edge.DestinationNode.Label == label);
            node.OriginNode.Edges.RemoveAll(edge => edge.To.Label == label);
        });
    }

    private void RemoveNodeFromMatrixIndex(string label)
        => AdjacentMatrix.RemoveAll(node => node.OriginNode.Label == label);

    public string LabelNode(int index)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index].IndexNode.Label,
            GraphType.Matrix => AdjacentMatrix[index].OriginNode.Label,
            _ => throw new NotImplementedException()
        };
    }

    public void PrintGraph()
    {
        switch (GraphType)
        {
            case GraphType.AdjacentList:
                PrintAdjacentList();
                break;
            case GraphType.Matrix:
                PrintAdjacentMatrix();
                break;
            default:
                throw new NotImplementedException("Unknown graph type");
        }
    }

    private void PrintAdjacentList()
    {
        Console.WriteLine("index | label | nós adjacentes");

        for (int i = 0; i < AdjacentList.Count; i++)
        {
            var node = AdjacentList[i];
            var adjacentLabels = string.Join(", ", node.AdjacentNodes.Select(n => n.Label));

            Console.WriteLine($"{i} - {node.IndexNode.Label} - [{adjacentLabels}]");
        }
    }

    private void PrintAdjacentMatrix()
    {
        if (AdjacentMatrix.Count == 0)
        {
            Console.WriteLine("A matriz está vazia.");
            return;
        }

        Console.Write("   ");
        for (int i = 0; i < AdjacentMatrix.Count; i++)
        {
            Console.Write($" {AdjacentMatrix[i].OriginNode.Label}");
        }
        Console.WriteLine();

        for (int i = 0; i < AdjacentMatrix.Count; i++)
        {
            Console.Write($"{AdjacentMatrix[i].OriginNode.Label}  ");

            for (int j = 0; j < AdjacentMatrix[i].Edges.Count; j++)
            {
                Console.Write($" {(AdjacentMatrix[i].Edges[j].IsConnected ? 1 : 0)}");
            }

            Console.WriteLine();
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

        if (GraphType == GraphType.Matrix)
            return AddToAdjacentMatrix(origin, destination);

        return AddToAdjacentList(origin, destination);
    }

    private Node GetNode(int index)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList[index].IndexNode,
            GraphType.Matrix => AdjacentMatrix[index].OriginNode,
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

    private bool AddToAdjacentMatrix(int origin, int destination)
    {
        if (IsDirected)
        {
            AdjacentMatrix[origin].Edges[destination] = new AdjacentMatrixEdge(AdjacentMatrix[destination].OriginNode, true);
            return true;
        }

        AdjacentMatrix[origin].Edges[destination] = new AdjacentMatrixEdge(AdjacentMatrix[destination].OriginNode, true);
        AdjacentMatrix[destination].Edges[origin] = new AdjacentMatrixEdge(AdjacentMatrix[origin].OriginNode, true);

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

        if (GraphType == GraphType.Matrix)
            return RemoveFromAdjacentMatrix(origin, destination);

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

    private bool RemoveFromAdjacentMatrix(int origin, int destination)
    {
        if (IsDirected)
        {
            AdjacentMatrix[origin].Edges[destination] = new AdjacentMatrixEdge(AdjacentMatrix[destination].OriginNode, false);
            return true;
        }

        AdjacentMatrix[origin].Edges[destination] = new AdjacentMatrixEdge(AdjacentMatrix[destination].OriginNode, false);
        AdjacentMatrix[destination].Edges[origin] = new AdjacentMatrixEdge(AdjacentMatrix[origin].OriginNode, false);

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

        var edge = GetNode(origin).Edges.FirstOrDefault(edge => edge.To.Label == LabelNode(destination));

        return edge == null ? -1 : edge.Weight;
    }

    public List<Node> GetAdjacentNodes(int index)
    {
        if (!GraphAlreadyHasNode(index, out _))
            return [];

        if (GraphType == GraphType.Matrix)
        {
            return AdjacentMatrix[index].Edges
                .Where(edge => edge.IsConnected)
                .Select(edge => edge.DestinationNode)
                .ToList();
        }

        return AdjacentList[index].AdjacentNodes;
    }
}