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

    private List<int> FindAugmentingPathWithDFS(int source, int sink)
    {
        var visited = new List<Node>();
        var parent = new Dictionary<Node, Node?>();
        var stack = new Stack<Node>();

        var sourceNode = GetNode(source);
        var sinkNode = GetNode(sink);

        stack.Push(sourceNode);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            if (currentNode == sinkNode)
                break;

            var currentIndex = GetNodeIndexByLabel(currentNode.Label);
            var adjacentNodes = GetAdjacentNodes(currentIndex);

            foreach (var adjNode in adjacentNodes)
            {
                var adjIndex = GetNodeIndexByLabel(adjNode.Label);
                var capacity = GetEdgeWeight(currentIndex, adjIndex);

                // Só considerar arestas com capacidade > 0
                if (!visited.Contains(adjNode) && capacity > 0)
                {
                    parent[adjNode] = currentNode;
                    stack.Push(adjNode);
                }
            }
        }

        // Se não conseguimos chegar ao destino, retornar caminho vazio
        if (!visited.Contains(sinkNode))
            return [];

        // Reconstruir o caminho
        var path = new List<int>();
        var current = sinkNode;

        while (current != null)
        {
            path.Add(GetNodeIndexByLabel(current.Label));
            parent.TryGetValue(current, out current);
        }

        path.Reverse();
        return path;
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

    private int GetNumOfVertices()
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Count,
            GraphType.Matrix => AdjacentMatrix.Count,
            _ => throw new NotImplementedException("Graph type not supported")
        };
    }

    private List<Node> GetVertices()
    {
        return GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Select(n => n.IndexNode).ToList(),
            GraphType.Matrix => AdjacentMatrix.Select(n => n.OriginNode).ToList(),
            _ => throw new NotImplementedException("Graph type not supported")
        };
    }

    public Dictionary<string, int> BruteForceColoring()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numColors = 2;
        var numVertices = GetNumOfVertices();

        if (numVertices == 0)
            return [];

        var vertices = GetVertices();

        while (numColors <= numVertices)
        {
            Console.WriteLine($"Tentando com {numColors} cores...");

            var coloring = new Dictionary<string, int>();

            foreach (var vertex in vertices)
                coloring[vertex.Label] = 1;

            var found = false;
            var moreColorings = true;

            while (moreColorings && !found)
            {
                if (IsValidColoring(coloring))
                {
                    found = true;
                    break;
                }

                moreColorings = NextColoring(coloring, vertices, numColors);
            }

            if (found)
            {
                stopWatch.Stop();

                Console.WriteLine($"Encontrada coloração válida usando {numColors} cores");
                Console.WriteLine($"\n⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");
                Console.WriteLine();

                return coloring;
            }

            numColors++;
        }

        Console.WriteLine("Não foi possível encontrar uma coloração válida");
        return [];
    }

    private bool NextColoring(Dictionary<string, int> coloring, List<Node> vertices, int numColors)
    {
        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            var label = vertices[i].Label;
            coloring[label]++;

            if (coloring[label] <= numColors)
                return true;

            coloring[label] = 1;
        }

        return false;
    }

    private bool IsValidColoring(Dictionary<string, int> coloring)
    {
        if (GraphType == GraphType.AdjacentList)
        {
            foreach (var node in AdjacentList)
            {
                int color = coloring[node.IndexNode.Label];

                foreach (var adjacentNode in node.AdjacentNodes)
                {
                    if (coloring[adjacentNode.Label] == color)
                        return false;
                }
            }
        }
        else
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

    public Dictionary<string, int> WelshPowellColoring()
    {
        Console.WriteLine("Aplicando algoritmo de coloração Welsh Powell...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numVertices = GraphType == GraphType.AdjacentList ? AdjacentList.Count : AdjacentMatrix.Count;

        if (numVertices == 0)
            return [];

        var vertices = GetVertices();

        var degrees = new Dictionary<Node, int>();

        foreach (var vertex in vertices)
        {
            int degree = GraphType == GraphType.AdjacentList
                ? AdjacentList.First(n => n.IndexNode == vertex).AdjacentNodes.Count
                : AdjacentMatrix.First(n => n.OriginNode == vertex).Edges.Count(e => e.IsConnected);

            degrees[vertex] = degree;
        }

        var sortedVertices = vertices.OrderByDescending(v => degrees[v]).ToList();

        var coloring = new Dictionary<string, int>();

        foreach (var vertex in vertices)
            coloring[vertex.Label] = 0;

        int currentColor = 1;

        while (coloring.Any(c => c.Value == 0))  // Enquanto existir um vértice sem cor no grafo
        {
            // Para cada vértice do grafo sem cor (seguindo a lista ordenada)
            foreach (var vertex in sortedVertices)
            {
                if (coloring[vertex.Label] != 0)  // Pula vértices já coloridos
                    continue;

                var canAssignColor = true;

                // Verifica se algum adjacente já tem a cor atual
                var adjacentNodes = GetAdjacentNodes(GetNodeIndexByLabel(vertex.Label));
                foreach (var adjNode in adjacentNodes)
                {
                    if (coloring.ContainsKey(adjNode.Label) && coloring[adjNode.Label] == currentColor)
                    {
                        canAssignColor = false;
                        break;
                    }
                }

                // Atribuir a cor atual caso não tenha vértice adjacente com a mesma cor
                if (canAssignColor)
                {
                    coloring[vertex.Label] = currentColor;
                }
            }

            currentColor++;  // Define a próxima cor não utilizada
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

        var numVertices = GetNumOfVertices();
        if (numVertices == 0)
            return [];

        var vertices = GetVertices();

        // 1. Ordenar os vértices pelo seu grau em ordem decrescente
        var degrees = new Dictionary<Node, int>();
        foreach (var vertex in vertices)
        {
            int degree = GraphType == GraphType.AdjacentList
                ? AdjacentList.First(n => n.IndexNode == vertex).AdjacentNodes.Count
                : AdjacentMatrix.First(n => n.OriginNode == vertex).Edges.Count(e => e.IsConnected);

            degrees[vertex] = degree;
        }

        // 2 e 3. Criar vetor de cores e inicializar todos os vértices como "sem cor"
        var coloring = new Dictionary<string, int>();
        var saturationDegree = new Dictionary<string, int>();
        var colored = new HashSet<string>();

        foreach (var vertex in vertices)
        {
            coloring[vertex.Label] = 0; // 0 significa "sem cor"
            saturationDegree[vertex.Label] = 0;
        }

        // 4. Colorir o vértice com maior grau com a primeira cor
        var maxDegreeVertex = vertices.OrderByDescending(v => degrees[v]).First();
        coloring[maxDegreeVertex.Label] = 1;
        colored.Add(maxDegreeVertex.Label);

        // Atualizar saturação dos vizinhos após colorir o primeiro vértice
        UpdateSaturation(maxDegreeVertex, saturationDegree, coloring, 1);

        // 5. Enquanto existir um vértice sem cor no grafo
        while (colored.Count < numVertices)
        {
            // Selecionar o vértice com maior grau de saturação
            // Em caso de empate, escolher o com maior grau dentre os de maior saturação
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

            // Atribuir para este vértice a primeira cor que não esteja em um vértice adjacente
            int color = GetFirstAvailableColor(nextVertex, coloring);
            coloring[nextVertex.Label] = color;
            colored.Add(nextVertex.Label);

            // Atualizar saturação dos vizinhos
            UpdateSaturation(nextVertex, saturationDegree, coloring, color);
        }

        // Código para exibir resultados
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
            if (coloring[adjNode.Label] == 0)
            {
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

    public Dictionary<string, int> SimpleColoring()
    {
        Console.WriteLine("Aplicando algoritmo de coloração simples (sem critério de ordem)...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var numVertices = GetNumOfVertices();

        if (numVertices == 0)
            return [];

        var vertices = GraphType == GraphType.AdjacentList
            ? AdjacentList.Select(n => n.IndexNode).ToList()
            : AdjacentMatrix.Select(n => n.OriginNode).ToList();

        var coloring = new Dictionary<string, int>();

        foreach (var vertex in vertices)
            coloring[vertex.Label] = 0;

        foreach (var vertex in vertices)
        {
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

    public int FordFulkerson(int source, int sink)
    {
        Console.WriteLine("Aplicando algoritmo de Ford-Fulkerson para encontrar fluxo máximo...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        int numVertices = GetNumOfVertices();
        if (source < 0 || source >= numVertices || sink < 0 || sink >= numVertices)
        {
            Console.WriteLine("Erro: Índices de origem ou destino inválidos.");
            return -1;
        }

        // Criar um grafo auxiliar (residual)
        var residualGraph = new Graph(true, true, GraphType);

        // Copiar todos os nós
        for (int i = 0; i < numVertices; i++)
            residualGraph.InsertNode(LabelNode(i));

        // Copiar todas as arestas com suas capacidades
        for (int i = 0; i < numVertices; i++)
        {
            var adjacentNodes = GetAdjacentNodes(i);
            foreach (var adjNode in adjacentNodes)
            {
                int j = GetNodeIndexByLabel(adjNode.Label);
                float capacity = GetEdgeWeight(i, j);
                residualGraph.AddEdge(i, j, capacity);
            }
        }

        int maxFlow = 0;  // Inicializar solução S
        int iterations = 0;

        Console.WriteLine("\nExecutando Ford-Fulkerson...");

        // Enquanto existir caminho aumentante
        while (true)
        {
            iterations++;

            // Encontrar um caminho aumentante de source para sink usando DFS
            var path = residualGraph.FindAugmentingPathWithDFS(source, sink);

            if (path.Count == 0)
                break;  // Não há mais caminhos aumentantes

            // Encontrar a capacidade mínima no caminho
            float minCapacity = float.MaxValue;
            for (int i = 0; i < path.Count - 1; i++)
            {
                int u = path[i];
                int v = path[i + 1];
                float capacity = residualGraph.GetEdgeWeight(u, v);
                minCapacity = Math.Min(minCapacity, capacity);
            }

            // Adicionar ao fluxo máximo
            maxFlow += (int)minCapacity;

            Console.WriteLine($"Iteração {iterations}: Encontrado caminho aumentante com capacidade {minCapacity}. Fluxo atual: {maxFlow}");

            // Apresentar o caminho
            Console.Write($"Caminho: {source}");

            for (int i = 1; i < path.Count; i++)
                Console.Write($" -> {path[i]}");

            Console.WriteLine();

            // Atualizar capacidades residuais
            for (int i = 0; i < path.Count - 1; i++)
            {
                int u = path[i];
                int v = path[i + 1];

                // Atualizar aresta direta (u,v)
                float forwardCapacity = residualGraph.GetEdgeWeight(u, v);
                residualGraph.RemoveEdge(u, v);

                if (forwardCapacity > minCapacity)
                    residualGraph.AddEdge(u, v, forwardCapacity - minCapacity);

                // Atualizar aresta reversa (v,u)
                var backwardExists = residualGraph.DoesEdgeExist(v, u);
                float backwardCapacity = backwardExists ? residualGraph.GetEdgeWeight(v, u) : 0;

                if (backwardExists)
                {
                    residualGraph.RemoveEdge(v, u);
                    residualGraph.AddEdge(v, u, backwardCapacity + minCapacity);
                }
                else
                {
                    // Criar aresta reversa se não existir
                    residualGraph.AddEdge(v, u, minCapacity);
                }
            }
        }

        stopWatch.Stop();
        Console.WriteLine($"\nFluxo máximo encontrado: {maxFlow}");
        Console.WriteLine($"Total de iterações: {iterations}");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        return maxFlow;
    }

    public void OptimizeMaxFlowWithLocalSearch(int source, int sink)
    {
        Console.WriteLine("Aplicando busca local para otimizar fluxo máximo...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        int numVertices = GetNumOfVertices();
        if (source < 0 || source >= numVertices || sink < 0 || sink >= numVertices)
        {
            Console.WriteLine("Erro: Índices de origem ou destino inválidos.");
            return;
        }

        // Calcular fluxo máximo da solução inicial
        int initialMaxFlow = FordFulkerson(source, sink);
        Console.WriteLine($"\nFluxo máximo da solução inicial: {initialMaxFlow}");

        // Criar uma cópia do grafo para trabalhar
        var workingGraph = CreateGraphCopy();

        int bestMaxFlow = initialMaxFlow;
        int steps = 0;
        bool improvement = true;

        Console.WriteLine("\nIniciando busca local...");

        while (improvement)
        {
            improvement = false;
            var edges = GetAllEdges();

            foreach (var edge in edges)
            {
                steps++;

                // Criar solução vizinha: inverter direção da aresta
                var neighborGraph = CreateNeighborSolution(workingGraph, edge.origin, edge.destination, edge.weight);

                // Calcular fluxo máximo da solução vizinha
                int neighborMaxFlow = neighborGraph.FordFulkerson(source, sink);

                Console.WriteLine($"Passo {steps}: Invertendo aresta {edge.origin}->{edge.destination}, Fluxo: {neighborMaxFlow}");

                // Se a solução vizinha é melhor, aceitar
                if (neighborMaxFlow > bestMaxFlow)
                {
                    bestMaxFlow = neighborMaxFlow;
                    workingGraph = neighborGraph;
                    improvement = true;
                    Console.WriteLine($"   ✓ Melhoria encontrada! Novo melhor fluxo: {bestMaxFlow}");
                    break; // First improvement strategy
                }
            }
        }

        stopWatch.Stop();

        Console.WriteLine("\n=== RESULTADO DA BUSCA LOCAL ===");
        Console.WriteLine($"Fluxo máximo da solução inicial: {initialMaxFlow}");
        Console.WriteLine($"Fluxo máximo da solução final: {bestMaxFlow}");
        Console.WriteLine($"Melhoria obtida: {bestMaxFlow - initialMaxFlow}");
        Console.WriteLine($"Número de passos: {steps}");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        if (bestMaxFlow > initialMaxFlow)
        {
            Console.WriteLine("\n✅ A busca local encontrou uma solução melhor!");
        }
        else
        {
            Console.WriteLine("\n❌ A busca local não encontrou melhorias.");
        }
    }

    private Graph CreateGraphCopy()
    {
        var copy = new Graph(IsDirected, IsWeighted, GraphType);

        // Copiar nós
        int numVertices = GetNumOfVertices();
        for (int i = 0; i < numVertices; i++)
        {
            copy.InsertNode(LabelNode(i));
        }

        // Copiar arestas
        for (int i = 0; i < numVertices; i++)
        {
            var adjacentNodes = GetAdjacentNodes(i);
            foreach (var adjNode in adjacentNodes)
            {
                int j = GetNodeIndexByLabel(adjNode.Label);
                float weight = GetEdgeWeight(i, j);
                copy.AddEdge(i, j, weight);
            }
        }

        return copy;
    }

    private Graph CreateNeighborSolution(Graph originalGraph, int origin, int destination, float weight)
    {
        var neighbor = originalGraph.CreateGraphCopy();

        // Remover aresta original
        neighbor.RemoveEdge(origin, destination);

        // Adicionar aresta com direção invertida
        neighbor.AddEdge(destination, origin, weight);

        return neighbor;
    }

    private List<(int origin, int destination, float weight)> GetAllEdges()
    {
        var edges = new List<(int origin, int destination, float weight)>();
        int numVertices = GetNumOfVertices();

        for (int i = 0; i < numVertices; i++)
        {
            var adjacentNodes = GetAdjacentNodes(i);
            foreach (var adjNode in adjacentNodes)
            {
                int j = GetNodeIndexByLabel(adjNode.Label);
                float weight = GetEdgeWeight(i, j);
                edges.Add((i, j, weight));
            }
        }

        return edges;
    }

    public void Prim()
    {
        Console.WriteLine("Aplicando algoritmo de Prim para árvore geradora mínima...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        int numVertices = GetNumOfVertices();
        if (numVertices == 0)
        {
            Console.WriteLine("Erro: O grafo está vazio.");
            return;
        }

        if (!IsWeighted)
        {
            Console.WriteLine("Erro: O algoritmo de Prim requer um grafo ponderado.");
            return;
        }

        // Verificar se há arestas com pesos negativos
        var hasNegativeWeights = GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Any(node => node.IndexNode.Edges.Any(edge => edge.Weight < 0)),
            GraphType.Matrix => AdjacentMatrix.Any(node => node.OriginNode.Edges.Any(edge => edge.Weight < 0)),
            _ => throw new NotImplementedException()
        };

        if (hasNegativeWeights)
        {
            Console.WriteLine("Aviso: O grafo contém arestas com pesos negativos.");
        }

        // S = conjunto vazio de arestas para a solução
        var solutionEdges = new List<(Node from, Node to, float weight)>();

        // Q = conjunto com todos os vértices do grafo para controle
        var qVertices = new HashSet<Node>(GetVertices());

        // Escolher vértice arbitrário A como inicial (primeiro vértice)
        var startVertex = qVertices.First();
        qVertices.Remove(startVertex);

        Console.WriteLine($"Vértice inicial escolhido: {startVertex.Label}");
        Console.WriteLine("\nExecutando algoritmo de Prim...");

        float totalWeight = 0;
        int iteration = 1;

        // Enquanto Q não estiver vazio
        while (qVertices.Count > 0)
        {
            Node? bestU = null;
            Node? bestV = null;
            float minWeight = float.MaxValue;

            // Encontrar a menor aresta {u,v} onde um pertence a Q e outro não
            foreach (var vertex in GetVertices())
            {
                // Se o vértice não está em Q (já foi processado)
                if (!qVertices.Contains(vertex))
                {
                    int vertexIndex = GetNodeIndexByLabel(vertex.Label);
                    var adjacentNodes = GetAdjacentNodes(vertexIndex);

                    foreach (var adjNode in adjacentNodes)
                    {
                        // Se o adjacente ainda está em Q
                        if (qVertices.Contains(adjNode))
                        {
                            int adjIndex = GetNodeIndexByLabel(adjNode.Label);
                            float edgeWeight = GetEdgeWeight(vertexIndex, adjIndex);

                            if (edgeWeight < minWeight)
                            {
                                minWeight = edgeWeight;
                                bestU = vertex;
                                bestV = adjNode;
                            }
                        }
                    }
                }
            }

            // Se encontrou uma aresta válida
            if (bestU != null && bestV != null)
            {
                // Adicionar aresta ao conjunto solução S
                solutionEdges.Add((bestU, bestV, minWeight));
                totalWeight += minWeight;

                // Remover do conjunto Q o vértice que pertencia a ele
                qVertices.Remove(bestV);

                Console.WriteLine($"Iteração {iteration}: Adicionada aresta {bestU.Label} -> {bestV.Label} (peso: {minWeight})");
                iteration++;
            }
            else
            {
                Console.WriteLine("Erro: Não foi possível encontrar uma aresta válida. O grafo pode não ser conectado.");
                break;
            }
        }

        stopWatch.Stop();

        // Exibir resultados
        Console.WriteLine("\n=== RESULTADO DO ALGORITMO DE PRIM ===");
        Console.WriteLine("Árvore Geradora Mínima encontrada:");

        foreach (var edge in solutionEdges)
        {
            Console.WriteLine($"  {edge.from.Label} -- {edge.to.Label} (peso: {edge.weight})");
        }

        Console.WriteLine($"\nNúmero total de arestas na solução: {solutionEdges.Count}");
        Console.WriteLine($"Soma total dos pesos das arestas: {totalWeight}");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        // Verificar se a solução é válida (deve ter n-1 arestas para n vértices)
        if (solutionEdges.Count == numVertices - 1)
        {
            Console.WriteLine("✅ Árvore geradora mínima encontrada com sucesso!");
        }
        else
        {
            Console.WriteLine("❌ Atenção: O número de arestas não corresponde a uma árvore geradora válida.");
            Console.WriteLine("   Isso pode indicar que o grafo não é conectado.");
        }
    }

    public void Kruskal()
    {
        Console.WriteLine("Aplicando algoritmo de Kruskal para árvore geradora mínima...");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        int numVertices = GetNumOfVertices();
        if (numVertices == 0)
        {
            Console.WriteLine("Erro: O grafo está vazio.");
            return;
        }

        if (!IsWeighted)
        {
            Console.WriteLine("Erro: O algoritmo de Kruskal requer um grafo ponderado.");
            return;
        }

        if (IsDirected)
        {
            Console.WriteLine("Erro: O algoritmo de Kruskal requer um grafo não direcionado.");
            return;
        }

        // Verificar se há arestas com pesos negativos
        var hasNegativeWeights = GraphType switch
        {
            GraphType.AdjacentList => AdjacentList.Any(node => node.IndexNode.Edges.Any(edge => edge.Weight < 0)),
            GraphType.Matrix => AdjacentMatrix.Any(node => node.OriginNode.Edges.Any(edge => edge.Weight < 0)),
            _ => throw new NotImplementedException()
        };

        if (hasNegativeWeights)
        {
            Console.WriteLine("Aviso: O grafo contém arestas com pesos negativos.");
        }

        // S = conjunto vazio de arestas para a solução
        var solutionEdges = new List<(int u, int v, string uLabel, string vLabel, float weight)>();

        // Q = conjunto com todas as arestas do grafo para controle
        var qEdges = GetAllEdgesForKruskal();

        // Ordenar arestas por peso (menor para maior)
        qEdges.Sort((e1, e2) => e1.weight.CompareTo(e2.weight));

        // F = floresta com cada vértice isolado sendo uma árvore (Union-Find usando arrays)
        var parent = new int[numVertices];
        var rank = new int[numVertices];

        // Inicializar cada vértice como sua própria árvore
        for (int i = 0; i < numVertices; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }

        Console.WriteLine($"Total de arestas no grafo: {qEdges.Count}");
        Console.WriteLine("\nExecutando algoritmo de Kruskal...");

        float totalWeight = 0;
        int iteration = 1;

        // Enquanto Q não estiver vazio
        foreach (var edge in qEdges)
        {
            // Seleciona a menor aresta {u, v} do conjunto Q
            // (já ordenado, então pegamos em ordem)

            // Se u e v pertencem a árvores diferentes no conjunto F
            if (!AreConnected(edge.u, edge.v, parent))
            {
                // Adiciona a aresta {u, v} para o conjunto S
                solutionEdges.Add(edge);
                totalWeight += edge.weight;

                // Une o conjunto das árvores que contém u e que contém v no conjunto F
                Union(edge.u, edge.v, parent, rank);

                Console.WriteLine($"Iteração {iteration}: Adicionada aresta {edge.uLabel} -- {edge.vLabel} (peso: {edge.weight})");
                iteration++;

                // Se já temos n-1 arestas, podemos parar (árvore completa)
                if (solutionEdges.Count == numVertices - 1)
                    break;
            }
        }

        stopWatch.Stop();

        // Exibir resultados
        Console.WriteLine("\n=== RESULTADO DO ALGORITMO DE KRUSKAL ===");
        Console.WriteLine("Árvore Geradora Mínima encontrada:");

        foreach (var edge in solutionEdges)
        {
            Console.WriteLine($"  {edge.uLabel} -- {edge.vLabel} (peso: {edge.weight})");
        }

        Console.WriteLine($"\nNúmero total de arestas na solução: {solutionEdges.Count}");
        Console.WriteLine($"Soma total dos pesos das arestas: {totalWeight}");
        Console.WriteLine($"⏰ Tempo de execução: {stopWatch.ElapsedMilliseconds} ms");

        // Verificar se a solução é válida (deve ter n-1 arestas para n vértices)
        if (solutionEdges.Count == numVertices - 1)
        {
            Console.WriteLine("✅ Árvore geradora mínima encontrada com sucesso!");
        }
        else
        {
            Console.WriteLine("❌ Atenção: O número de arestas não corresponde a uma árvore geradora válida.");
            Console.WriteLine("   Isso pode indicar que o grafo não é conectado.");
        }
    }

    private List<(int u, int v, string uLabel, string vLabel, float weight)> GetAllEdgesForKruskal()
    {
        var edges = new List<(int u, int v, string uLabel, string vLabel, float weight)>();
        var addedEdges = new HashSet<string>(); // Para evitar duplicatas em grafos não direcionados
        int numVertices = GetNumOfVertices();

        for (int i = 0; i < numVertices; i++)
        {
            var adjacentNodes = GetAdjacentNodes(i);
            foreach (var adjNode in adjacentNodes)
            {
                int j = GetNodeIndexByLabel(adjNode.Label);

                // Para grafos não direcionados, evitar arestas duplicadas
                // Criar uma chave única para a aresta (menor índice primeiro)
                string edgeKey = i < j ? $"{i}-{j}" : $"{j}-{i}";

                if (!addedEdges.Contains(edgeKey))
                {
                    float weight = GetEdgeWeight(i, j);
                    edges.Add((i, j, LabelNode(i), LabelNode(j), weight));
                    addedEdges.Add(edgeKey);
                }
            }
        }

        return edges;
    }

    // Encontrar o representante (raiz) do conjunto que contém x (Union-Find)
    private int Find(int x, int[] parent)
    {
        if (parent[x] != x)
        {
            // Compressão de caminho para otimização
            parent[x] = Find(parent[x], parent);
        }
        return parent[x];
    }

    // Unir os conjuntos que contêm x e y (Union-Find)
    private void Union(int x, int y, int[] parent, int[] rank)
    {
        int rootX = Find(x, parent);
        int rootY = Find(y, parent);

        if (rootX != rootY)
        {
            // União por rank para otimização
            if (rank[rootX] < rank[rootY])
            {
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }
    }

    // Verificar se x e y estão no mesmo conjunto (conectados) (Union-Find)
    private bool AreConnected(int x, int y, int[] parent)
    {
        return Find(x, parent) == Find(y, parent);
    }
}