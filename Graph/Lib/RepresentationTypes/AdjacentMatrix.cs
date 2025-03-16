namespace Lib.RepresentationTypes;

public sealed record AdjacentMatrixNode(
    Node OriginNode,
    List<AdjacentMatrixEdge> Edges
);

public record AdjacentMatrixEdge(
    Node DestinationNode,
    bool IsConnected
);