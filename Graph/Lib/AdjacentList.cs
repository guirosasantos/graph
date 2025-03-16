namespace Lib;

public sealed record AdjacentListNode(
    Node CurrentNode,
    List<Node> AdjacentNodes
);