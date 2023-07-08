namespace Icfpc2023.Domain;

public class Pillar
{
    public Pillar(int id, PointDto position, double radius)
    {
        Id = id;
        Position = position;
        Radius = radius;
    }

    public int Id { get; }

    public PointDto Position { get; }

    public double Radius { get; }

    public bool DoesBlocks(Musician other, Listener listener)
    {
        var otherVector = other.Position - listener.Position;
        var otherDistance = Math.Sqrt(otherVector * otherVector);
        var thisVector = Position - listener.Position;
        var distance = Math.Sqrt(thisVector * thisVector);

        var distanceToLine = Math.Abs((other.Position.X - listener.Position.X) * (listener.Position.Y - Position.Y)
                                      - (listener.Position.X - Position.X) * (other.Position.Y - listener.Position.Y))
                             / Math.Sqrt(otherVector * otherVector);

        return distanceToLine < Radius && (distance - Radius < otherDistance);
    }
}