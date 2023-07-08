namespace Icfpc2023.Utils;

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

    public double GetHappiness(IReadOnlyCollection<Musician> musicians)
    {
        var happiness = 0d;

        foreach (var musician in musicians)
        {
            var isBlocked = musicians.Where(m => m.Id != musician.Id)
                .Aggregate(false, (s, m) => s || m.DoesBlocks(musician, this));

            if (isBlocked)
            {
                continue;
            }

            var vector = musician.Position - Position;

            var distance = vector * vector;

            happiness += Math.Ceiling(1000000 * Tastes[musician.Instrument].Value / distance);
        }

        return happiness;
    }
}