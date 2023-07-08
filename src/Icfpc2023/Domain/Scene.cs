namespace Icfpc2023.Domain;

public record Scene
{
    public PointDto BottomLeft { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }
}