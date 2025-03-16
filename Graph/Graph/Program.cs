using Lib;
using Lib.Enuns;

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
            RemoveNode(graph);
            break;
        case 5:
            AddEdge(graph);
            break;
        case 6:
            RemoveEdge(graph);
            break;
        case 7:
            CheckEdgeExists(graph);
            break;
        case 8:
            GetEdgeWeight(graph);
            break;
        case 9:
            GetAdjacentNodes(graph);
            break;
        case 10:
            PrintGraph(graph);
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
    Console.WriteLine("4 - Remover nó");
    Console.WriteLine("5 - Adicionar aresta");
    Console.WriteLine("6 - Remover aresta");
    Console.WriteLine("7 - Verificar se aresta existe");
    Console.WriteLine("8 - Obter peso da aresta");
    Console.WriteLine("9 - Obter nós adjacentes");
    Console.WriteLine("10 - Imprimir grafo");
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
    Console.WriteLine("2 - Matriz (não implementado)");
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