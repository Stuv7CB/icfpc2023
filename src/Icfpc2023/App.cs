using System.Threading.Channels;
using Icfpc2023.Api;
using Icfpc2023.Domain;
using ShellProgressBar;

namespace Icfpc2023;

public class App : IDisposable
{
    private readonly IReadOnlyCollection<Problem> _problems;
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _semaphoreSlim = new (10);

    private const double _temperature = 1000d;
    private const double _step = 10d;

    public App(IReadOnlyCollection<Problem> problems, ApiClient apiClient)
    {
        _problems = problems;
        _apiClient = apiClient;
    }

    public async Task Calculate(ChannelWriter<(int, Problem)> problemWriter, ChannelWriter<Placements> solutionWriter)
    {
        Console.WriteLine("Input next problem to solve or 0 for all");
        var problemNumber = int.Parse(Console.ReadLine());

        var problemsToSolve = (problemNumber == 0
            ? _problems.Zip(Enumerable.Range(1, int.MaxValue))
            : _problems.Zip(Enumerable.Range(1, int.MaxValue)).Skip(problemNumber - 1).Take(1)).ToArray();

        if (problemNumber != 0)
        {
            await problemWriter.WriteAsync((problemNumber, _problems.ElementAt(problemNumber - 1)));
        }

        using var pBar = new ProgressBar(
            problemsToSolve.Length,
            $"Processing");

        var result = await Task.WhenAll(problemsToSolve
            .Select(async problem => await ProcessProblem(
                problem.First,
                _apiClient,
                problem.Second,
                pBar))
            .ToArray());

        if (problemNumber != 0)
        {
            await solutionWriter.WriteAsync(result.Single().Placements);
        }
    }

    private async Task<(int problemId, Placements Placements)> ProcessProblem(
        Problem problem,
        ApiClient apiClient,
        int problemId,
        ProgressBar pBar)
    {
        try
        {
            await Task.Yield();
            await _semaphoreSlim.WaitAsync();

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

            var pillars = problem.Pillars.Select((p, i) =>
                new Domain.Pillar(i, new PointDto(p.Center.First(), p.Center.Last()), p.Radius)).ToArray();

            var calculator = new ScoreCalculator(problemId, scene, listeners, pillars);

            using var childBar = pBar.Spawn(
                (int)(_temperature / _step),
                $"[{problemId}] Start processing");

            var solver = new Solver(_temperature, _step);
            var progress = new Progress<double>(_ => childBar.Tick());
            var score = solver.Solve(calculator, scene, listeners, musicians, progress);

            Console.WriteLine($"[{problemId}] Resulting score is {score}");

            var placement = new Placements
            {
                PlacementsList = musicians.Select(m => new Coords
                {
                    X = m.Position.X,
                    Y = m.Position.Y
                }).ToList()
            };

            await apiClient.Submit((uint)problemId, placement);

            pBar.Tick();

            return (problemId, placement);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public void Dispose()
    {
        _apiClient.Dispose();
        _semaphoreSlim.Dispose();
    }
}