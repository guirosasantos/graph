using System.Threading.Tasks;
using Lib.Enuns;

namespace Lib.Extensions;

public static class GraphFromFileExtension
{
    public static async Task<Graph> FromFileAsync(string filePath, bool isDirected, bool isWeighted, GraphType graphType)
    {
        var graph = new Graph(isDirected, isWeighted, graphType);

        var lines = await File.ReadAllLinesAsync(filePath);
        var firstLine = lines[0].Split(',');
        var (numberOfNodes, numberOfEdges) = (int.Parse(firstLine[0]), int.Parse(firstLine[1]));

        var nodes = GetNodes(lines, numberOfNodes);

        foreach (var node in nodes)
            graph.InsertNode(node);

        var edges = GetEdges(lines, numberOfNodes, numberOfEdges, isWeighted);

        foreach (var (originNode, destinationNode, weight) in edges)
            graph.AddEdge(originNode, destinationNode, weight);

        return graph;
    }

    private static List<string> GetNodes(string[] lines, int numberOfNodes)
    {
        var nodes = new List<string>();
        for (int i = 1; i <= numberOfNodes; i++)
        {
            var splitedLine = lines[i].Split(',');
            var originNode = splitedLine[0];
            var destinationNode = splitedLine[1];
            nodes.Add(originNode);
            nodes.Add(destinationNode);
        }

        return [.. nodes.Distinct().OrderBy(n => n)];
    }

    private static List<(int originNode, int destinationNode, int weight)> GetEdges(string[] lines, int numberOfNodes,
        int numberOfEdges, bool isWeighted)
    {
        var edges = new List<(int originNode, int destinationNode, int weight)>();
        for (int i = numberOfNodes + 1; i <= numberOfEdges + numberOfNodes; i++)
        {
            var splitedLine = lines[i].Split(',');
            var originNode = int.Parse(splitedLine[0]);
            var destinationNode = int.Parse(splitedLine[1]);
            var weight = isWeighted ? int.Parse(splitedLine[2]) : 1;
            edges.Add((originNode, destinationNode, weight));
        }

        return edges;
    }
}