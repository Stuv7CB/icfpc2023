namespace Icfpc2023.Utils;

public class Musician
{
    public Musician(int id, Instrument instrument)
    {
        Id = id;
        Instrument = instrument;
    }
    public int Id { get; }

    public PointDto Position { get; private set; }

    public Instrument Instrument { get; }

    public bool DoesBlocks(Musician other, Listener listener)
    {
        var otherVector = other.Position - listener.Position;
        var otherDistance = Math.Sqrt(otherVector * otherVector);
        var thisVector = Position - listener.Position;
        var distance = Math.Sqrt(thisVector * thisVector);

        var distanceToLine = Math.Abs((other.Position.X - listener.Position.X) * (listener.Position.Y - Position.Y)
                                      - (listener.Position.X - Position.X) * (other.Position.Y - listener.Position.Y))
                             / Math.Sqrt(otherVector * otherVector);

        return distanceToLine <= 5 && (distance - 5.0 <= otherDistance);
    }

    public void AdjustPosition(PointDto point)
    {
        Position = point;
    }
}