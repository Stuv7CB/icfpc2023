using Icfpc2023.Utils;

namespace Icfpc2023;

public class ScoreCalculator
{
    public double CalculateScore(Scene scene, IReadOnlyCollection<Listener> listeners,
        IReadOnlyCollection<Musician> musicians)
    {
        foreach (var musician in musicians)
        {
            if (musician.Position.X >= scene.BottomLeft.X + scene.Width - 10)
            {
                return double.MinValue;
            }

            if (musician.Position.X <= scene.BottomLeft.X + 10)
            {
                return double.MinValue;
            }

            if (musician.Position.Y >= scene.BottomLeft.Y + scene.Height - 10)
            {
                return double.MinValue;
            }

            if (musician.Position.Y <= scene.BottomLeft.Y +  10)
            {
                return double.MinValue;
            }

            foreach (var otherMusician in musicians.Where(m => m.Id != musician.Id))
            {
                var vector = otherMusician.Position - musician.Position;
                if (Math.Sqrt(vector * vector) <= 10)
                {
                    return double.MinValue;
                }
            }
        }

        return listeners.Aggregate(0d, (i, listener) => i + listener.GetHappiness(musicians));
    }
}