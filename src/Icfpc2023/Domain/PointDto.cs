namespace Icfpc2023.Utils;

public record PointDto(double X, double Y)
{
    public static double operator *(PointDto a, PointDto b)
        => a.X * b.X + a.Y * b.Y;

    public static PointDto operator *(PointDto a, double b)
        => new(a.X * b, a.Y * b);

    public static PointDto operator +(PointDto a, PointDto b)
        => new(a.X + b.X, a.Y + b.Y);

    public static PointDto operator -(PointDto a, PointDto b)
        => new(a.X - b.X, a.Y - b.Y);
};