namespace Lib;

public sealed record Edge(
    Node From,
    Node To,
    int Weight
);