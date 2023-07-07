namespace Icfpc2023.Utils;

public record PointDto
{
    public float X { get; init; }

    public float Y { get; init; }

    public static float operator *(PointDto a, PointDto b)
        => a.X * b.X + a.Y * b.Y;

    public static PointDto operator +(PointDto a, PointDto b)
        => new()
        {
            X = a.X + b.X,
            Y = a.Y + b.Y
        };

    public static PointDto operator -(PointDto a, PointDto b)
        => new()
        {
            X = a.X - b.X,
            Y = a.Y - b.Y
        };
};