using System.Globalization;
using Lib.Enuns;
using Lib.Extensions;
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

    public void DepthSearch(int origin)
    {
        if (!GraphAlreadyHasNode(origin, out _))
        {
            Console.WriteLine("O nó de origem não existe no grafo.");
            return;
        }

        var originNode = GetNode(origin);
        var visited = new List<Node>();
        var stack = new Stack<Node>();
        stack.Push(originNode);

        Console.WriteLine("Caminho percorrido:");

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);
            Console.WriteLine($"Visitando: {currentNode.Label}");

            var adjacentNodes = GraphType switch
            {
                GraphType.AdjacentList => currentNode.Edges.Select(edge => edge.To),
                GraphType.Matrix => GetAdjacentNodes(AdjacentList.IndexOf(AdjacentList.First(n => n.IndexNode == currentNode))),
                _ => throw new NotImplementedException()
            };

            foreach (var node in adjacentNodes)
            {
                if (!visited.Contains(node))
                    stack.Push(node);
            }
        }
    }

    public void BreadthFirstSearch(in int origin)
    {
        Node originNode;

        try
        {
            originNode = GetNode(origin);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("O nó de origem não existe no grafo.");
            return;
        }

        var auxiliaryQueue = new Queue<Node>();
        var visited = new List<Node>();

        auxiliaryQueue.Enqueue(originNode);
        visited.Add(originNode);

        while (auxiliaryQueue.Count != 0)
        {
            var currentNode = auxiliaryQueue.Dequeue();

            var currentNodeIndex = GraphFromFileExtension.GetIndex(currentNode.Label);

            var adjacentNodes = GetAdjacentNodes(currentNodeIndex).OrderBy(node => node.Label);

            TryAddNodesToDataStructures(adjacentNodes, visited, auxiliaryQueue);
        }

        Console.WriteLine("Caminho percorrido:");
        foreach (var node in visited)
        {
            Console.WriteLine($"Visitando: {node.Label}");
        }

        static void TryAddNodesToDataStructures(IEnumerable<Node> adjacentNodes, List<Node> visited,
            Queue<Node> auxiliaryQueue)
        {
            foreach (var node in adjacentNodes)
            {
                if (!visited.Contains(node))
                {
                    visited.Add(node);
                    auxiliaryQueue.Enqueue(node);
                }
            }
        }
    }


    public void Dijkstra(int origin)
    {
        if (!IsWeighted)
        {
            Console.WriteLine("Erro: O algoritmo de Dijkstra requer um grafo ponderado.");
            return;
        }

        if (!GraphAlreadyHasNode(origin, out _))
        {
            Console.WriteLine("O nó de origem não existe no grafo.");
            return;
        }
        
        var hasNegativeWeights = GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Any(node => node.IndexNode.Edges.Any(edge => edge.Weight < 0)),
            GraphType.Matrix => AdjacentMatrix.Any(node => node.OriginNode.Edges.Any(edge => edge.Weight < 0)),
            _ => throw new NotImplementedException()
        };

        if (hasNegativeWeights)
        {
            Console.WriteLine("Erro: O algoritmo de Dijkstra não suporta arestas com pesos negativos.");
            return;
        }

        var distances = new Dictionary<Node, int>();
        var previous = new Dictionary<Node, Node?>();
        var priorityQueue = new SortedSet<(int Distance, Node Node)>(Comparer<(int, Node)>.Create((a, b) =>
        {
            int compare = a.Item1.CompareTo(b.Item1);
            return compare == 0 ? string.Compare(a.Item2.Label, b.Item2.Label, StringComparison.Ordinal) : compare;
        }));

        var originNode = GetNode(origin);

        foreach (var node in GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode)
            : AdjacentMatrix.Select(n => n.OriginNode))
        {
            distances[node] = int.MaxValue;
            previous[node] = null;
        }

        distances[originNode] = 0;
        priorityQueue.Add((0, originNode));

        while (priorityQueue.Count > 0)
        {
            var (currentDistance, currentNode) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);
            
            var neighbors = GraphType switch
            {
                GraphType.AdjacentList => currentNode.Edges,
                GraphType.Matrix => AdjacentMatrix
                    .First(n => n.OriginNode == currentNode)
                    .Edges
                    .Where(e => e.IsConnected)
                    .Select(e => new Edge(currentNode, e.DestinationNode,
                        GetEdgeWeight(AdjacentMatrix.IndexOf(AdjacentMatrix.First(n => n.OriginNode == currentNode)),
                            AdjacentMatrix.IndexOf(AdjacentMatrix.First(n => n.OriginNode == e.DestinationNode))))),
                _ => throw new NotImplementedException()
            };

            foreach (var edge in neighbors)
            {
                var neighbor = edge.To;
                var newDistance = currentDistance + edge.Weight;

                if (newDistance < distances[neighbor])
                {
                    priorityQueue.Remove((distances[neighbor], neighbor));
                    distances[neighbor] = newDistance;
                    previous[neighbor] = currentNode;
                    priorityQueue.Add((newDistance, neighbor));
                }
            }
        }
        
        Console.WriteLine("Menores distâncias a partir do nó de origem:");
        foreach (var node in distances.Keys)
        {
            var path = new Stack<string>();
            var current = node;

            while (current != null)
            {
                path.Push(current.Label);
                current = previous[current];
            }

            Console.WriteLine($"Nó: {node.Label}, Distância: {distances[node]}, Caminho: {string.Join(" -> ", path)}");
        }
    }
}