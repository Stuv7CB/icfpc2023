namespace Icfpc2023.Domain;

public class Listener
{
    public Listener(int id, PointDto position, IDictionary<Instrument, Taste> tastes)
    {
        Id = id;
        Position = position;
        Tastes = tastes;
    }

    public int Id { get; }

    public PointDto Position { get; }

    public IDictionary<Instrument, Taste> Tastes { get; }

    public double GetHappinessForMusician(Musician musician)
    {
        var vector = musician.Position - Position;

        var distance = vector * vector;

        return Math.Ceiling(1000000 * Tastes[musician.Instrument].Value / distance);
    }
}