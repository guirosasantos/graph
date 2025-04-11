using Lib;
using Lib.Enuns;
using Lib.Extensions;

Graph? graph = null;
bool exit = false;

Console.WriteLine("Bem-vindo ao programa de gerenciamento de grafos!");

while (!exit)
{
    PrintMenu(graph);
    var option = GetOption();

    switch (option)
    {
        case 1:
            graph = CreateGraph(graph);
            break;
        case 2:
            InsertNode(graph);
            break;
        case 3:
            InsertNodeBatch(graph);
            break;
        case 4:
            AddPresetNodes(graph);
            break;
        case 5:
            RemoveNode(graph);
            break;
        case 6:
            AddEdge(graph);
            break;
        case 7:
            RemoveEdge(graph);
            break;
        case 8:
            CheckEdgeExists(graph);
            break;
        case 9:
            GetEdgeWeight(graph);
            break;
        case 10:
            GetAdjacentNodes(graph);
            break;
        case 11:
            PrintGraph(graph);
            break;
        case 12:
            graph = await ReadGraphFromFileAsync();
            break;
        case 13:
            DepthSearch(graph);
            break;
        case 14:
            RunDijkstra(graph);
            break;
        case 99:
            graph = null;
            break;
        case 0:
            exit = true;
            Console.WriteLine("Saindo do programa. Até mais!");
            break;
        default:
            Console.WriteLine("Opção inválida. Por favor, tente novamente.");
            break;
    }

    if (!exit)
    {
        Console.WriteLine("\nPressione qualquer tecla para continuar...");
        Console.ReadKey();
        Console.Clear();
    }
}

static void PrintMenu(Graph? programGraph)
{
    PrintTitle(programGraph);
    Console.WriteLine("1 - Criar um novo grafo");
    Console.WriteLine("2 - Inserir nó");
    Console.WriteLine("3 - Inserir nós em lote");
    Console.WriteLine("4 - Inserir nós pré-definidos");
    Console.WriteLine("5 - Remover nó");
    Console.WriteLine("6 - Adicionar aresta");
    Console.WriteLine("7 - Remover aresta");
    Console.WriteLine("8 - Verificar se aresta existe");
    Console.WriteLine("9 - Obter peso da aresta");
    Console.WriteLine("10 - Obter nós adjacentes");
    Console.WriteLine("11 - Imprimir grafo");
    Console.WriteLine("12 - Ler grafo de arquivo");
    Console.WriteLine("13 - Executar busca em profundidade");
    Console.WriteLine("14 - Executar algoritmo de Dijkstra");
    Console.WriteLine("99 - Remover grafo atual");
    Console.WriteLine("0 - Sair");
    Console.Write("\nEscolha uma opção: ");
}

static void PrintTitle(Graph? graph)
{
    string graphInfo = string.Empty;

    if (graph != null)
    {
        string representation = graph.GraphType == GraphType.AdjacentList ? "Lista Adjacente" : "Matriz Adjacente";
        string direction = graph.IsDirected ? "Direcionado" : "Não direcionado";
        string weight = graph.IsWeighted ? "Ponderado" : "Não ponderado";

        graphInfo = $"{representation} | {direction} | {weight}";
    }

    Console.WriteLine($"\nMENU DE OPERAÇÕES DO GRAFO {graphInfo}");
}

static int GetOption()
{
    if (int.TryParse(Console.ReadLine(), out int option))
        return option;
    return -1;
}

static Graph CreateGraph(Graph? programGraph)
{
    if (programGraph != null)
    {
        Console.WriteLine("Erro: Já existe um grafo criado. Remova-o antes de criar um novo.");
        return programGraph;
    }

    Console.WriteLine("\n--- Criação de um novo grafo ---");

    Console.Write("O grafo é direcionado? (S/N): ");
    bool isDirected = Console.ReadLine()?.ToUpper() == "S";

    Console.Write("O grafo é ponderado? (S/N): ");
    bool isWeighted = Console.ReadLine()?.ToUpper() == "S";

    Console.WriteLine("Escolha o tipo de representação:");
    Console.WriteLine("1 - Lista de Adjacência");
    Console.WriteLine("2 - Matriz Adjacente");
    Console.Write("Tipo: ");

    GraphType graphType = int.TryParse(Console.ReadLine(), out int type) && type == 2
        ? GraphType.Matrix
        : GraphType.AdjacentList;

    var graph = new Graph(isDirected, isWeighted, graphType);
    Console.WriteLine("Grafo criado com sucesso!");
    return graph;
}

static void InsertNode(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o rótulo do nó a ser inserido: ");
    var label = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(label))
    {
        Console.WriteLine("Erro: Rótulo inválido.");
        return;
    }

    bool success = graph.InsertNode(label);
    if (success)
        Console.WriteLine($"Nó '{label}' inserido com sucesso!");
    else
        Console.WriteLine($"Erro: Não foi possível inserir o nó '{label}'. Talvez ele já exista.");
}

static void InsertNodeBatch(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite os rótulos dos nós a serem inseridos (separados por vírgula): ");
    var labels = Console.ReadLine()?.Split(',');

    if (labels == null || labels.Length == 0)
    {
        Console.WriteLine("Erro: Rótulos inválidos.");
        return;
    }

    int count = 0;
    foreach (var label in labels)
    {
        if (string.IsNullOrWhiteSpace(label))
            continue;

        bool success = graph.InsertNode(label.Trim());
        if (success)
            count++;
    }

    Console.WriteLine($"{count} nós inseridos com sucesso!");
}

static void AddPresetNodes(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.WriteLine("\nInserindo nós pré-definidos...");
    graph.InsertNode("A");
    graph.InsertNode("B");
    graph.InsertNode("C");
    graph.InsertNode("D");
    graph.InsertNode("E");
    graph.InsertNode("F");

    Console.WriteLine("Nós inseridos com sucesso!");

    Console.WriteLine("\nAdicionando arestas...");

    if (graph.IsWeighted)
    {
        AddPresetWeightedEdges(graph);
        Console.WriteLine("Arestas ponderadas adicionadas com sucesso!");
        return;
    }

    AddPresetUnweightedEdges(graph);
    Console.WriteLine("Arestas não ponderadas adicionadas com sucesso!");
}

static void AddPresetWeightedEdges(Graph graph)
{
    if (graph.IsDirected)
    {
        AddPresetWeightedDirectedEdges(graph);
        return;
    }

    AddPresetWeightedUndirectedEdges(graph);
}

static void AddPresetWeightedDirectedEdges(Graph graph)
{
    //A
    graph.AddEdge(0, 1, 1);
    graph.AddEdge(0, 2, 2);
    graph.AddEdge(0, 3, 3);
    graph.AddEdge(0, 4, 4);
    graph.AddEdge(0, 5, 5);

    //B
    graph.AddEdge(1, 0, 6);
    graph.AddEdge(1, 2, 7);
    graph.AddEdge(1, 5, 8);

    //C
    graph.AddEdge(2, 0, 9);
    graph.AddEdge(2, 1, 10);
    graph.AddEdge(2, 3, 11);

    //D
    graph.AddEdge(3, 0, 12);
    graph.AddEdge(3, 2, 13);
    graph.AddEdge(3, 4, 14);

    //E
    graph.AddEdge(4, 0, 15);
    graph.AddEdge(4, 3, 16);
    graph.AddEdge(4, 5, 17);

    //F
    graph.AddEdge(5, 0, 18);
    graph.AddEdge(5, 2, 19);
    graph.AddEdge(5, 4, 20);
}

static void AddPresetWeightedUndirectedEdges(Graph graph)
{
    //A
    graph.AddEdge(0, 1, 1);
    graph.AddEdge(0, 2, 2);
    graph.AddEdge(0, 3, 3);
    graph.AddEdge(0, 4, 4);
    graph.AddEdge(0, 5, 5);

    //B
    graph.AddEdge(1, 2, 6);
    graph.AddEdge(1, 5, 7);

    //C
    graph.AddEdge(2, 3, 8);

    //D
    graph.AddEdge(3, 4, 9);

    //E
    graph.AddEdge(4, 5, 10);
}

static void AddPresetUnweightedEdges(Graph graph)
{
    if (graph.IsDirected)
    {
        AddPresetDirectedEdges(graph);
        return;
    }

    AddPresetUndirectedEdges(graph);
}

static void AddPresetUndirectedEdges(Graph graph)
{
    //A
    graph.AddEdge(0, 1);
    graph.AddEdge(0, 2);
    graph.AddEdge(0, 3);
    graph.AddEdge(0, 4);
    graph.AddEdge(0, 5);

    //B
    graph.AddEdge(1, 0);
    graph.AddEdge(1, 2);
    graph.AddEdge(1, 5);

    //C
    graph.AddEdge(2, 0);
    graph.AddEdge(2, 1);
    graph.AddEdge(2, 3);

    //D
    graph.AddEdge(3, 0);
    graph.AddEdge(3, 2);
    graph.AddEdge(3, 4);

    //E
    graph.AddEdge(4, 0);
    graph.AddEdge(4, 3);
    graph.AddEdge(4, 5);

    //F
    graph.AddEdge(5, 0);
    graph.AddEdge(5, 2);
    graph.AddEdge(5, 4);
}

static void AddPresetDirectedEdges(Graph graph)
{
    //A
    graph.AddEdge(0, 1);
    graph.AddEdge(0, 2);
    graph.AddEdge(0, 3);
    graph.AddEdge(0, 4);
    graph.AddEdge(0, 5);

    //B
    graph.AddEdge(1, 2);
    graph.AddEdge(1, 5);

    //C
    graph.AddEdge(2, 3);

    //D
    graph.AddEdge(3, 4);

    //E
    graph.AddEdge(4, 5);
}

static void RemoveNode(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó a ser removido: ");
    if (!int.TryParse(Console.ReadLine(), out int index))
    {
        Console.WriteLine("Erro: Índice inválido.");
        return;
    }

    bool success = graph.RemoveNode(index);
    if (success)
        Console.WriteLine($"Nó no índice {index} removido com sucesso!");
    else
        Console.WriteLine($"Erro: Não foi possível remover o nó no índice {index}.");
}

static void AddEdge(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.Write("Digite o índice do nó de destino: ");
    if (!int.TryParse(Console.ReadLine(), out int destination))
    {
        Console.WriteLine("Erro: Índice de destino inválido.");
        return;
    }

    int weight = 1;
    if (graph.IsWeighted)
    {
        Console.Write("Digite o peso da aresta: ");
        if (!int.TryParse(Console.ReadLine(), out weight))
        {
            Console.WriteLine("Erro: Peso inválido. Usando o valor padrão (1).");
            weight = 1;
        }
    }

    bool success = graph.AddEdge(origin, destination, weight);
    if (success)
        Console.WriteLine($"Aresta adicionada com sucesso de {origin} para {destination}!");
    else
        Console.WriteLine($"Erro: Não foi possível adicionar a aresta de {origin} para {destination}.");
}

static void RemoveEdge(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.Write("Digite o índice do nó de destino: ");
    if (!int.TryParse(Console.ReadLine(), out int destination))
    {
        Console.WriteLine("Erro: Índice de destino inválido.");
        return;
    }

    bool success = graph.RemoveEdge(origin, destination);
    if (success)
        Console.WriteLine($"Aresta removida com sucesso de {origin} para {destination}!");
    else
        Console.WriteLine($"Erro: Não foi possível remover a aresta de {origin} para {destination}.");
}

static void CheckEdgeExists(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.Write("Digite o índice do nó de destino: ");
    if (!int.TryParse(Console.ReadLine(), out int destination))
    {
        Console.WriteLine("Erro: Índice de destino inválido.");
        return;
    }

    bool exists = graph.DoesEdgeExist(origin, destination);
    Console.WriteLine(exists
        ? $"A aresta de {origin} para {destination} existe."
        : $"A aresta de {origin} para {destination} não existe.");
}

static void GetEdgeWeight(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    if (!graph.IsWeighted)
    {
        Console.WriteLine("Erro: O grafo não é ponderado.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.Write("Digite o índice do nó de destino: ");
    if (!int.TryParse(Console.ReadLine(), out int destination))
    {
        Console.WriteLine("Erro: Índice de destino inválido.");
        return;
    }

    int weight = graph.GetEdgeWeight(origin, destination);
    if (weight == -1)
        Console.WriteLine($"Erro: Não foi possível obter o peso da aresta de {origin} para {destination}.");
    else
        Console.WriteLine($"O peso da aresta de {origin} para {destination} é {weight}.");
}

static void GetAdjacentNodes(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó: ");
    if (!int.TryParse(Console.ReadLine(), out int index))
    {
        Console.WriteLine("Erro: Índice inválido.");
        return;
    }

    var adjacentNodes = graph.GetAdjacentNodes(index);
    if (adjacentNodes.Count == 0)
    {
        Console.WriteLine($"O nó no índice {index} não tem nós adjacentes.");
        return;
    }

    Console.WriteLine($"\nNós adjacentes ao nó no índice {index} ({graph.LabelNode(index)}):");
    foreach (var node in adjacentNodes)
    {
        Console.WriteLine($"- {node.Label}");
    }
}

static void PrintGraph(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.WriteLine("\n--- Estrutura do Grafo ---");
    graph.PrintGraph();
}

static void DepthSearch(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem para a busca em profundidade: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.WriteLine("\nExecutando busca em profundidade...");
    graph.DepthSearch(origin);
}

static void RunDijkstra(Graph? graph)
{
    if (graph == null)
    {
        Console.WriteLine("Erro: Nenhum grafo foi criado. Crie um grafo primeiro.");
        return;
    }

    if (!graph.IsWeighted)
    {
        Console.WriteLine("Erro: O algoritmo de Dijkstra requer um grafo ponderado.");
        return;
    }

    Console.Write("\nDigite o índice do nó de origem: ");
    if (!int.TryParse(Console.ReadLine(), out int origin))
    {
        Console.WriteLine("Erro: Índice de origem inválido.");
        return;
    }

    Console.WriteLine("\nExecutando o algoritmo de Dijkstra...");
    graph.Dijkstra(origin);
}

static async Task<Graph> ReadGraphFromFileAsync()
{
    Console.WriteLine("\n--- Leitura de grafo a partir de arquivo ---");

    Console.WriteLine("Escolha o tipo de representação:");
    Console.WriteLine("1 - Lista de Adjacência");
    Console.WriteLine("2 - Matriz Adjacente");
    Console.Write("Tipo: ");

    GraphType graphType = int.TryParse(Console.ReadLine(), out int type) && type == 2
        ? GraphType.Matrix
        : GraphType.AdjacentList;

    var filePath = GetFilePath();

    return await GraphFromFileExtension.FromFileAsync(filePath, graphType);
}

static string GetFilePath()
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var projectDirectory = GetProjectDirectory(currentDirectory);

    var filePath = $"{projectDirectory}\\Grafo.csv";

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Erro: O arquivo '{filePath}' não foi encontrado.");
        return string.Empty;
    }

    return filePath;
}

static string GetProjectDirectory(string currentDirectory)
{
    if (IsInDebugMode())
        return Directory.GetParent(currentDirectory)?.Parent?.Parent?.FullName!;

    return Directory.GetCurrentDirectory();
}

static bool IsInDebugMode()
    => AppDomain.CurrentDomain.FriendlyName.Contains("vshost");