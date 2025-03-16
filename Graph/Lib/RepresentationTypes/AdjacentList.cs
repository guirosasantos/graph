namespace Lib.RepresentationTypes;

public sealed record AdjacentListNode(
    Node IndexNode,
    List<Node> AdjacentNodes
);