using Icfpc2023.Domain;

namespace Icfpc2023;

public class ScoreCalculator
{
    private readonly int _problemId;
    private readonly Scene _scene;
    private readonly IReadOnlyCollection<Listener> _listeners;

    public ScoreCalculator(int problemId, Scene scene, IReadOnlyCollection<Listener> listeners)
    {
        _problemId = problemId;
        _scene = scene;
        _listeners = listeners;
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
            return _listeners.Select(l => musicians
                    .Aggregate(0d, (d, musician) =>
                        d + Math.Ceiling(l.GetHappinessForMusician(musician, musicians) * musician.GetClosenessFactor(musicians))))
                .Sum();
        }

        return _listeners.Select(l => musicians
                .Aggregate(0d, (d, musician) =>
                    d + l.GetHappinessForMusician(musician, musicians)))
            .Sum();
    }
}