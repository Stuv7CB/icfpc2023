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

    public float GetHappiness(IReadOnlyCollection<Musicion> musitions)
    {
        var happiness = 0f;

        foreach (var musition in musitions)
        {
            var isBlocked = musitions.Where(m => m.Id != musition.Id)
                .Aggregate(false, (s, m) => s || m.DoesBlocks(musition, this));

            if (isBlocked)
            {
                continue;
            }

            var vector = musition.Position - Position;

            var distance = vector * vector;

            happiness += (float)Math.Ceiling(1000000 * Tastes[musition.Instrument].Value / distance);
        }

        return happiness;
    }
}