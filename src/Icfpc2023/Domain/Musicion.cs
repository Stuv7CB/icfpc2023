namespace Icfpc2023.Utils;

public class Musicion
{
    public Musicion(int id, Instrument instrument)
    {
        Id = id;
        Instrument = instrument;
    }
    public int Id { get; }

    public PointDto Position { get; private set; }

    public Instrument Instrument { get; }

    public bool DoesBlocks(Musicion other, Listener listener)
    {
        var otherVector = other.Position - listener.Position;

        var distanceToLine = Math.Abs((other.Position.X - listener.Position.X) * (listener.Position.Y - Position.Y)
                                      - (listener.Position.X - Position.X) * (other.Position.Y - listener.Position.Y))
                             / Math.Sqrt(otherVector * otherVector);

        return distanceToLine <= 5;
    }
}