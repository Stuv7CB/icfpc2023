using Icfpc2023.Api;
using Icfpc2023.Domain;
using ShellProgressBar;

namespace Icfpc2023;

public class App
{
    private List<Problem> _problems;
    private Dictionary<int, Placements> _solutions = new();
    private Render _render;
    private ApiClient _apiClient;
    private Thread _calculationTrhead;
    private Thread _inputThread;
    private int _renderProblemId = 1;
    private static object _lock = new();

    private const double _temperature = 1000d;
    private const double _step = 10d;   
    public App(List<Problem> problems, Render render, ApiClient apiClient)
    {
        _problems = problems;
        _render = render;
        _apiClient = apiClient;
        _calculationTrhead = new(new ThreadStart(calculate));
        _inputThread = new(new ThreadStart(input));
        _calculationTrhead.Start();
        _inputThread.Start();
    }
    private void input()
    {
        while (true)
        {
            Console.WriteLine("Choose a problem:");
            var input = Console.ReadLine();
            lock(_lock)
            {
                _renderProblemId = Int32.Parse(input);
                _render.setProblem(_problems.ElementAt(_renderProblemId), _renderProblemId);
                if (_solutions.ContainsKey(_renderProblemId))
                {
                    _render.setSolution(_solutions[_renderProblemId]);
                }
            }
        }

    }
    private async void calculate()
    {
        using var pBar = new ProgressBar(
            _problems.Count,
            $"Processing");

        using var semaphore = new SemaphoreSlim(10);

        var result = await Task.WhenAll(_problems.Zip(Enumerable.Range(1, int.MaxValue))
            .Select(async problem => await ProcessProblem(
                problem.First,
                _apiClient,
                problem.Second,
                pBar,
                semaphore))
            .ToArray());
        lock(_lock)
        {
            foreach (var solution in result)
            {
                _solutions.Add(solution.problemId, solution.Placements);
            }
            _render.setSolution(result.ElementAt(_renderProblemId - 1).Placements);
        }
    }
    private static async Task<(int problemId, Placements Placements)> ProcessProblem(Problem problem, ApiClient apiClient, int problemId, ProgressBar pBar, SemaphoreSlim semaphore)
    {
        try
        {
            await Task.Yield();
            await semaphore.WaitAsync();

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

        var calculator = new ScoreCalculator(problemId, scene, listeners);

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
            semaphore.Release();
        }
    }
}