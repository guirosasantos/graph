namespace Lib.RepresentationTypes;

public sealed record AdjacentMatrixNode(
    Node OriginNode,
    Node DestinationNode,
    bool IsDirected
);