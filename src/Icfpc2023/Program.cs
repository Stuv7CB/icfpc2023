using Icfpc2023.Api;
using Icfpc2023.Utils;

namespace Icfpc2023;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var random = new Random();

        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
        using var apiClient = new ApiClient(apiToken);
        var problems = await apiClient.GetProblemsDefinition();
        var problemId = 22;

        foreach (var problem in problems.Skip(problemId - 1))
        {
            var numberOfInstruments = problem.Musicians.Max() + 1;

            var instruments = Enumerable.Range(0, (int)numberOfInstruments)
                .Select(i => new Instrument((uint)i))
                .ToDictionary(i => i.Id);

            var musicians = problem.Musicians
                .Select((instrument, i) => new Musician(i, instruments[instrument]))
                .ToArray();

            var scene = new Scene
            {
                Height = problem.StageHeight,
                Width = problem.StageWidth,
                BottomLeft = new PointDto
                (
                    problem.StageBottomLeft.First(),
                    problem.StageBottomLeft.Last()
                )
            };

            var xMiddle = scene.BottomLeft.X + scene.Width / 2;
            var yMiddle = scene.BottomLeft.Y + scene.Height / 2;


            foreach (var musician in musicians)
            {
                musician.AdjustPosition(new PointDto
                (
                    xMiddle,
                    yMiddle
                ));
            }

            var listeners = problem.Attendees
                .Select((a, i) => new Listener(
                    i,
                    new PointDto(a.X, a.Y),
                    a.Tastes.Select((t, j) => (t, j)).ToDictionary(
                        kv => instruments[(uint)kv.j],
                        kv => new Taste
                        {
                            Instrument = instruments[(uint)kv.j],
                            Value = kv.t
                        }))).ToArray();

            var calculator = new ScoreCalculator();

            var solver = new Solver(10f, 100);
            var score = solver.Solve(calculator, scene, listeners, musicians);
            Console.WriteLine($"Resulting score is {score}");

            if (score > 0)
            {
                await apiClient.Submit((uint)problemId, new Placements
                {
                    PlacementsList = musicians.Select(m => new Coords
                    {
                        X = m.Position.X,
                        Y = m.Position.Y
                    }).ToList()
                });
            }

            problemId++;
        }
    }
}