using System.Threading.Tasks;
using Lib.Enuns;

namespace Lib.Extensions;

public static class GraphFromFileExtension
{
    public static async Task<Graph> FromFileAsync(string filePath, GraphType graphType)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var firstLine = lines[0].Split(',');
        var numberOfNodes = int.Parse(firstLine[0]);
        var numberOfEdges = int.Parse(firstLine[1]);
        var isDirected = ConvertToBoolean(firstLine[2]);
        var isWeighted = ConvertToBoolean(firstLine[3]);

        var graph = new Graph(isDirected, isWeighted, graphType);

        var nodes = GetNodes(lines, numberOfNodes);

        foreach (var node in nodes)
            graph.InsertNode(node);

        var edges = GetEdges(lines, numberOfEdges, isWeighted);

        foreach (var (originNode, destinationNode, weight) in edges)
            graph.AddEdge(originNode, destinationNode, weight);

        return graph;
    }

    private static bool ConvertToBoolean(string value)
        => value switch
        {
            "0" => false,
            "1" => true,
            _ => throw new ArgumentException("Invalid value for boolean conversion")
        };

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

    private static List<(int originNode, int destinationNode, int weight)> GetEdges(string[] lines, int numberOfEdges,
        bool isWeighted)
    {
        var edges = new List<(int originNode, int destinationNode, int weight)>();
        for (int i = 1; i <= numberOfEdges; i++)
        {
            var splitedLine = lines[i].Split(',');
            var originNode = GetIndex(splitedLine[0]);
            var destinationNode = GetIndex(splitedLine[1]);
            var weight = isWeighted ? int.Parse(splitedLine[2]) : 1;
            edges.Add((originNode, destinationNode, weight));
        }

        return edges;
    }

    public static int GetIndex(string label)
        => label.ToUpper() switch
        {
            "A" => 0,
            "B" => 1,
            "C" => 2,
            "D" => 3,
            "E" => 4,
            "F" => 5,
            "G" => 6,
            "H" => 7,
            "I" => 8,
            "J" => 9,
            "K" => 10,
            "L" => 11,
            "M" => 12,
            "N" => 13,
            "O" => 14,
            "P" => 15,
            "Q" => 16,
            "R" => 17,
            "S" => 18,
            "T" => 19,
            "U" => 20,
            "V" => 21,
            "W" => 22,
            "X" => 23,
            "Y" => 24,
            "Z" => 25,
            _ => throw new ArgumentException("Invalid node name")
        };
}