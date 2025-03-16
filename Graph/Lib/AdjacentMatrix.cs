namespace Lib;

public sealed record AdjacentMatrixNode(
    Node OriginNode,
    Node DestinationNode,
    bool IsDirected
);