using System.Text.RegularExpressions;
using Lib.Enuns;

namespace Lib.Extensions;

public static partial class GraphFromFileExtension
{
    public static async Task<Graph> FromFileAsync(string filePath, GraphType graphType)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var firstLine = lines[0].Split(' ');
        var numberOfLines = lines.Length;
        var numberOfEdges = int.Parse(firstLine[1]);
        var isDirected = ConvertToBoolean(firstLine[2]);
        var isWeighted = ConvertToBoolean(firstLine[3]);

        var graph = new Graph(isDirected, isWeighted, graphType);

        var nodes = GetNodes(lines, numberOfLines);

        foreach (var node in nodes)
            graph.InsertNode(node);

        var edges = GetEdges(lines, numberOfEdges, isWeighted, graph);

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

    private static List<string> GetNodes(string[] lines, int numberOfLines)
    {
        var nodes = new List<string>();
        for (int i = 1; i <= numberOfLines-1; i++)
        {
            var splitedLine = lines[i].Split(' ');
            var originNode = splitedLine[0];
            var destinationNode = splitedLine[1];
            nodes.Add(originNode);
            nodes.Add(destinationNode);
        }

        return [.. nodes.Distinct().OrderBy(n =>
        {
            var intN = int.Parse(n);
            return intN;
        })];
    }

    private static List<(int originNode, int destinationNode, float weight)> GetEdges(string[] lines, int numberOfEdges,
        bool isWeighted, Graph graph)
    {
        var edges = new List<(int originNode, int destinationNode, float weight)>();
        for (int i = 1; i <= numberOfEdges; i++)
        {
            var splitedLine = lines[i].Split(' ');
            var originNode = graph.GetNodeIndexByLabel(splitedLine[0]);
            var destinationNode = graph.GetNodeIndexByLabel(splitedLine[1]);
            var weight = isWeighted ? ConvertFromLyra(splitedLine[2]) : 1;
            edges.Add((originNode, destinationNode, weight));
        }

        return edges;
    }

    private static float ConvertFromLyra(string value)
    {
        if (!FollowsLyraPattern(value))
            return float.Parse(value);

        value = value.Replace('.', ',');
        value = $"0{value}";

        return float.Parse(value);
    }

    private static bool FollowsLyraPattern(in string value)
        => LyraPattern().IsMatch(value);

    [GeneratedRegex(@"^\.[0-9]+$")]
    private static partial Regex LyraPattern();
}