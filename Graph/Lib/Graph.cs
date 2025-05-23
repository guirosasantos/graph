using System.Diagnostics;
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

    public bool AddEdge(int origin, int destination, float weight = 1)
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

    private bool AddEdgeToGraph(Node originNode, Node destinationNode, float weight)
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

    public float GetEdgeWeight(int origin, int destination)
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

            var currentNodeIndex = GetNodeIndexByLabel(currentNode.Label);

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

        var distances = new Dictionary<Node, float>();
        var previous = new Dictionary<Node, Node?>();
        var priorityQueue = new SortedSet<(float Distance, Node Node)>(Comparer<(float, Node)>.Create((a, b) =>
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

    internal int GetNodeIndexByLabel(string label)
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.FindIndex(node => node.IndexNode.Label == label),
            GraphType.Matrix => AdjacentMatrix.FindIndex(node => node.OriginNode.Label == label),
            _ => throw new NotImplementedException("Graph type not supported")
        };
    }

    public Dictionary<string, int> BruteForceColoring()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numColors = 2;
        var numVertices = GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count;

        if (numVertices == 0)
            return new Dictionary<string, int>();

        var vertices = GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode).ToList()
            : AdjacentMatrix.Select(n => n.OriginNode).ToList();

        while (numColors <= numVertices)
        {
            Console.WriteLine($"Tentando com {numColors} cores...");

            // Try all possible colorings with numColors
            var coloring = new Dictionary<string, int>();
            foreach (var vertex in vertices)
            {
                coloring[vertex.Label] = 1; // Start with all vertices having color 1
            }

            // Try all combinations
            bool found = false;
            bool moreColorings = true;

            while (moreColorings && !found)
            {
                // Check if current coloring is valid
                if (IsValidColoring(coloring))
                {
                    found = true;
                    break;
                }

                // Get the next coloring
                moreColorings = NextColoring(coloring, vertices, numColors);
            }

            if (found)
            {
                Console.WriteLine($"Encontrada coloração válida usando {numColors} cores");

                stopWatch.Stop();

                Console.WriteLine($"\n⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");
                Console.WriteLine();

                return coloring;
            }

            // No valid coloring found with this number of colors, increase
            numColors++;
        }

        // Should never reach here for simple graphs, but just in case
        Console.WriteLine("Não foi possível encontrar uma coloração válida");
        return new Dictionary<string, int>();
    }

    private bool NextColoring(Dictionary<string, int> coloring, List<Node> vertices, int numColors)
    {
        // This is like incrementing a number in base-numColors
        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            string label = vertices[i].Label;
            coloring[label]++;

            if (coloring[label] <= numColors)
            {
                return true; // Successfully moved to next coloring
            }

            // Overflow, reset this digit and try to increment the next one
            coloring[label] = 1;
        }

        // If we get here, we've gone through all colorings
        return false;
    }

    private bool IsValidColoring(Dictionary<string, int> coloring)
    {
        // Check if any adjacent vertices have the same color
        if (GraphType == GraphType.AdjacentList)
        {
            foreach (var node in AdjacentList)
            {
                int color = coloring[node.IndexNode.Label];

                foreach (var adjacentNode in node.AdjacentNodes)
                {
                    if (coloring[adjacentNode.Label] == color)
                    {
                        return false;
                    }
                }
            }
        }
        else // Matrix
        {
            for (int i = 0; i < AdjacentMatrix.Count; i++)
            {
                int color = coloring[AdjacentMatrix[i].OriginNode.Label];

                for (int j = 0; j < AdjacentMatrix[i].Edges.Count; j++)
                {
                    if (AdjacentMatrix[i].Edges[j].IsConnected &&
                        coloring[AdjacentMatrix[i].Edges[j].DestinationNode.Label] == color)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void PrintColoring(Dictionary<string, int> coloring, in int verticesCount)
    {
        if (coloring == null || coloring.Count == 0)
        {
            Console.WriteLine("Nenhuma coloração válida encontrada.");
            return;
        }

        if (verticesCount >= 10)
            return;

        Console.WriteLine("Resultado da coloração do grafo:");
        foreach (var entry in coloring.OrderBy(e => GetNodeIndexByLabel(e.Key)))
        {
            Console.WriteLine($"Nó {entry.Key}: Cor {entry.Value}");
        }
    }

    public Dictionary<string, int> ColorGraph()
    {
        Console.WriteLine("Aplicando algoritmo de coloração por força bruta...");
        var coloring = BruteForceColoring();
        PrintColoring(coloring, GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count);
        return coloring;
    }

    // Welsh Powell Algorithm
    public Dictionary<string, int> WelshPowellColoring()
    {
        Console.WriteLine("Aplicando algoritmo de coloração Welsh Powell...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numVertices = GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count;

        if (numVertices == 0)
            return new Dictionary<string, int>();

        // Get vertices
        var vertices = GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode).ToList()
            : AdjacentMatrix.Select(n => n.OriginNode).ToList();

        // Calculate degree of each vertex
        var degrees = new Dictionary<Node, int>();
        foreach (var vertex in vertices)
        {
            int degree = GraphType == GraphType.AdjacentList
                ? AdjacentList.First(n => n.IndexNode == vertex).AdjacentNodes.Count
                : AdjacentMatrix.First(n => n.OriginNode == vertex).Edges.Count(e => e.IsConnected);

            degrees[vertex] = degree;
        }

        // Sort vertices by degree in descending order
        var sortedVertices = vertices.OrderByDescending(v => degrees[v]).ToList();

        // Initialize coloring
        var coloring = new Dictionary<string, int>();
        foreach (var vertex in vertices)
        {
            coloring[vertex.Label] = 0; // 0 means uncolored
        }

        int currentColor = 1;

        // Color vertices
        while (coloring.Any(c => c.Value == 0))
        {
            // For each uncolored vertex
            foreach (var vertex in sortedVertices)
            {
                if (coloring[vertex.Label] != 0)
                    continue;

                // Check if we can assign current color
                bool canAssignColor = true;

                var adjacentNodes = GetAdjacentNodes(GetNodeIndexByLabel(vertex.Label));
                foreach (var adjNode in adjacentNodes)
                {
                    if (coloring.ContainsKey(adjNode.Label) && coloring[adjNode.Label] == currentColor)
                    {
                        canAssignColor = false;
                        break;
                    }
                }

                if (canAssignColor)
                {
                    coloring[vertex.Label] = currentColor;
                }
            }

            currentColor++;
        }

        stopWatch.Stop();

        int colorsUsed = coloring.Values.Max();
        Console.WriteLine($"Welsh Powell usou {colorsUsed} cores.");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        PrintColoring(coloring, numVertices);
        return coloring;
    }

    // DSATUR Algorithm
    public Dictionary<string, int> DSATURColoring()
    {
        Console.WriteLine("Aplicando algoritmo de coloração DSATUR...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numVertices = GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count;

        if (numVertices == 0)
            return new Dictionary<string, int>();

        // Get vertices
        var vertices = GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode).ToList()
            : AdjacentMatrix.Select(n => n.OriginNode).ToList();

        // Calculate degree of each vertex
        var degrees = new Dictionary<Node, int>();
        foreach (var vertex in vertices)
        {
            int degree = GraphType == GraphType.AdjacentList
                ? AdjacentList.First(n => n.IndexNode == vertex).AdjacentNodes.Count
                : AdjacentMatrix.First(n => n.OriginNode == vertex).Edges.Count(e => e.IsConnected);

            degrees[vertex] = degree;
        }

        // Initialize coloring and saturation degrees
        var coloring = new Dictionary<string, int>();
        var saturationDegree = new Dictionary<string, int>();
        var colored = new HashSet<string>();

        foreach (var vertex in vertices)
        {
            coloring[vertex.Label] = 0; // 0 means uncolored
            saturationDegree[vertex.Label] = 0;
        }

        // Find vertex with max degree and color it first
        var maxDegreeVertex = vertices.OrderByDescending(v => degrees[v]).First();
        coloring[maxDegreeVertex.Label] = 1;
        colored.Add(maxDegreeVertex.Label);

        // Update saturation of neighbors
        UpdateSaturation(maxDegreeVertex, saturationDegree, coloring, 1);

        // Color remaining vertices
        while (colored.Count < numVertices)
        {
            // Find vertex with highest saturation degree
            Node nextVertex = null;
            int maxSaturation = -1;
            int maxDegree = -1;

            foreach (var vertex in vertices)
            {
                if (colored.Contains(vertex.Label))
                    continue;

                int sat = saturationDegree[vertex.Label];
                int deg = degrees[vertex];

                if (sat > maxSaturation || (sat == maxSaturation && deg > maxDegree))
                {
                    maxSaturation = sat;
                    maxDegree = deg;
                    nextVertex = vertex;
                }
            }

            // Assign color to selected vertex
            int color = GetFirstAvailableColor(nextVertex, coloring);
            coloring[nextVertex.Label] = color;
            colored.Add(nextVertex.Label);

            // Update saturation of neighbors
            UpdateSaturation(nextVertex, saturationDegree, coloring, color);
        }

        stopWatch.Stop();

        int colorsUsed = coloring.Values.Max();
        Console.WriteLine($"DSATUR usou {colorsUsed} cores.");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        PrintColoring(coloring, numVertices);
        return coloring;
    }

    private void UpdateSaturation(Node vertex, Dictionary<string, int> saturationDegree,
                                 Dictionary<string, int> coloring, int color)
    {
        var adjacentNodes = GetAdjacentNodes(GetNodeIndexByLabel(vertex.Label));

        foreach (var adjNode in adjacentNodes)
        {
            if (coloring[adjNode.Label] == 0) // If uncolored
            {
                // Check if this color is already counted in saturation
                var adjAdjNodes = GetAdjacentNodes(GetNodeIndexByLabel(adjNode.Label));
                bool alreadyCounted = adjAdjNodes.Any(n => coloring.ContainsKey(n.Label) && coloring[n.Label] == color);

                if (!alreadyCounted)
                {
                    saturationDegree[adjNode.Label]++;
                }
            }
        }
    }

    private int GetFirstAvailableColor(Node vertex, Dictionary<string, int> coloring)
    {
        var adjacentNodes = GetAdjacentNodes(GetNodeIndexByLabel(vertex.Label));

        var usedColors = new HashSet<int>();

        foreach (var adjNode in adjacentNodes)
        {
            if (coloring.ContainsKey(adjNode.Label) && coloring[adjNode.Label] > 0)
                usedColors.Add(coloring[adjNode.Label]);
        }

        var color = 1;

        while (usedColors.Contains(color))
            color++;

        return color;
    }

    // Simple coloring algorithm with arbitrary ordering
    public Dictionary<string, int> SimpleColoring()
    {
        Console.WriteLine("Aplicando algoritmo de coloração simples (sem critério de ordem)...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numVertices = GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count;

        if (numVertices == 0)
            return new Dictionary<string, int>();

        // Get vertices (using original order)
        var vertices = GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode).ToList()
            : AdjacentMatrix.Select(n => n.OriginNode).ToList();

        // Initialize coloring
        var coloring = new Dictionary<string, int>();

        foreach (var vertex in vertices)
            coloring[vertex.Label] = 0;

        // Color vertices in arbitrary order (original order)
        foreach (var vertex in vertices)
        {
            // Find first available color
            int color = GetFirstAvailableColor(vertex, coloring);
            coloring[vertex.Label] = color;
        }

        stopWatch.Stop();

        int colorsUsed = coloring.Values.Max();
        Console.WriteLine($"Coloração simples usou {colorsUsed} cores.");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        PrintColoring(coloring, numVertices);
        return coloring;
    }

    public void RunAllColoringAlgorithms()
    {
        Console.WriteLine("\n=== COMPARAÇÃO DE ALGORITMOS DE COLORAÇÃO ===\n");

        Console.WriteLine("1. Coloração por Força Bruta");
        var bruteForceTiming = MeasureExecutionTime(() => ColorGraph());

        Console.WriteLine("\n2. Coloração Welsh Powell");
        var welshPowellTiming = MeasureExecutionTime(() => WelshPowellColoring());

        Console.WriteLine("\n3. Coloração DSATUR");
        var dsaturTiming = MeasureExecutionTime(() => DSATURColoring());

        Console.WriteLine("\n4. Coloração Simples (sem critério de ordem)");
        var simpleTiming = MeasureExecutionTime(() => SimpleColoring());

        Console.WriteLine("\n=== RESUMO DE TEMPOS DE EXECUÇÃO ===");
        Console.WriteLine($"Força Bruta: {bruteForceTiming} ms");
        Console.WriteLine($"Welsh Powell: {welshPowellTiming} ms");
        Console.WriteLine($"DSATUR: {dsaturTiming} ms");
        Console.WriteLine($"Coloração Simples: {simpleTiming} ms");
    }

    private long MeasureExecutionTime(Func<Dictionary<string, int>> coloringAlgorithm)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        coloringAlgorithm();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}