using Icfpc2023.Domain;

namespace Icfpc2023;

public class ScoreCalculator
{
    private readonly int _problemId;
    private readonly Scene _scene;
    private readonly IReadOnlyCollection<Listener> _listeners;
    private readonly IReadOnlyCollection<Pillar> _pillars;

    public ScoreCalculator(int problemId, Scene scene, IReadOnlyCollection<Listener> listeners, IReadOnlyCollection<Pillar> pillars)
    {
        _problemId = problemId;
        _scene = scene;
        _listeners = listeners;
        _pillars = pillars;
    }

    public double CalculateScore(IReadOnlyCollection<Musician> musicians)
    {
        foreach (var musician in musicians)
        {
            if (musician.Position.X > _scene.BottomLeft.X + _scene.Width - 10)
            {
                return double.MinValue;
            }

            if (musician.Position.X < _scene.BottomLeft.X + 10)
            {
                return double.MinValue;
            }

            if (musician.Position.Y > _scene.BottomLeft.Y + _scene.Height - 10)
            {
                return double.MinValue;
            }

            if (musician.Position.Y < _scene.BottomLeft.Y + 10)
            {
                return double.MinValue;
            }

            foreach (var otherMusician in musicians.Where(m => m.Id != musician.Id))
            {
                var vector = otherMusician.Position - musician.Position;
                if (Math.Sqrt(vector * vector) < 10)
                {
                    return double.MinValue;
                }
            }
        }

        if (_problemId >= 56)
        {
            return _listeners.Select(l => CalculateTotalScoreForListenerV2(l, musicians, _pillars)).Sum();
        }

        return _listeners.Aggregate(0d, (d, l) => d + CalculateTotalScoreForListener(l, musicians, _pillars));
    }

    private double CalculateTotalScoreForListener(Listener listener, IReadOnlyCollection<Musician> musicians,
        IReadOnlyCollection<Pillar> pillars)
    {
        var happiness = 0d;

        foreach (var musician in musicians)
        {
            var isBlocked = musicians.Where(m => m.Id != musician.Id)
                .Aggregate(false, (s, m) => s || m.DoesBlocks(musician, listener));

            var isBlockedByColumn = pillars.Aggregate(false, (p, m) => p || m.DoesBlocks(musician, listener));

            if (isBlocked || isBlockedByColumn)
            {
                happiness += 0d;
                continue;
            }

            happiness += listener.GetHappinessForMusician(musician);
        }

        return happiness;
    }

    private double CalculateTotalScoreForListenerV2(Listener listener, IReadOnlyCollection<Musician> musicians,
        IReadOnlyCollection<Pillar> pillars)
    {
        var happiness = 0d;

        foreach (var musician in musicians)
        {
            var isBlocked = musicians.Where(m => m.Id != musician.Id)
                .Aggregate(false, (s, m) => s || m.DoesBlocks(musician, listener));

            var isBlockedByColumn = pillars.Aggregate(false, (p, m) => p || m.DoesBlocks(musician, listener));

            if (isBlocked || isBlockedByColumn)
            {
                happiness += 0d;
                continue;
            }

            happiness += Math.Ceiling(musician.GetClosenessFactor(musicians) *listener.GetHappinessForMusician(musician));
        }

        return happiness;
    }
}