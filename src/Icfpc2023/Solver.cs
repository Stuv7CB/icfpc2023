using Icfpc2023.Domain;
using ShellProgressBar;

namespace Icfpc2023;

public class Solver
{
    private readonly double _temperature;
    private readonly double _step;

    public Solver(double temperature, double step)
    {
        _temperature = temperature;
        _step = step;
    }

    public double Solve(ScoreCalculator calculator, Scene scene, IReadOnlyCollection<Listener> listeners,
        IReadOnlyCollection<Musician> musicians, IProgress<double> progress)
    {
        var rand = new Random();

        var temperature = _temperature;

        var score = calculator.CalculateScore(musicians);
        while (temperature > 0)
        {
            progress.Report(temperature);

            var origPos = new Queue<PointDto>();

            foreach (var musician in musicians)
            {
                origPos.Enqueue(musician.Position);

                var needBreak = false;
                while (!needBreak)
                {
                    needBreak = true;
                    var position = new PointDto(
                        scene.BottomLeft.X + 10 + rand.NextDouble() * (scene.Width - 20),
                        scene.BottomLeft.Y + 10 + rand.NextDouble() * (scene.Height - 20));

                    musician.AdjustPosition(position);

                    foreach (var otherMusician in musicians.Where(m => m.Id != musician.Id))
                    {
                        var vector = otherMusician.Position - musician.Position;
                        if (Math.Sqrt(vector * vector) <= 10)
                        {
                            needBreak = false;
                            break;
                        }
                    }
                }
            }

            var newScore = calculator.CalculateScore(musicians);
            if (Math.Abs(newScore - double.MinValue) < double.Epsilon)
            {
                Console.WriteLine("Hooray");
            }

            if (newScore >= score)
            {
                score = newScore;
                temperature -= _step;
                continue;
            }

            var prob = Math.Exp((newScore - score) / temperature);
            if (rand.NextDouble() < prob)
            {
                score = newScore;
            }
            else
            {
                foreach (var musician in musicians)
                {
                    musician.AdjustPosition(origPos.Dequeue());
                }
            }

            temperature -= _step;
        }

        return score;
    }
}