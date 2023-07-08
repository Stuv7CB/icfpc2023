using Icfpc2023.Utils;

namespace Icfpc2023;

public class Solver
{
    private static double H = 10;

    private readonly double _step;
    private readonly int _iterations;

    public Solver(double step, int iterations)
    {
        _step = step;
        _iterations = iterations;
    }

    public double Solve(ScoreCalculator calculator, Scene scene, IReadOnlyCollection<Listener> listeners,
        IReadOnlyCollection<Musician> musicians)
    {
        var rand = new Random();

        var temperature = 1000d;
        var step = 1d;

        var score = calculator.CalculateScore(scene, listeners, musicians);

        while (temperature > 0)
        {
            Console.WriteLine(temperature);

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

            var newScore = calculator.CalculateScore(scene, listeners, musicians);
            if (Math.Abs(newScore - double.MinValue) < double.Epsilon)
            {
                Console.WriteLine("Hooray");
            }

            if (newScore >= score)
            {
                score = newScore;
                temperature -= step;
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

            temperature -= step;
        }

        return score;
    }
}