﻿using Icfpc2023.Api;
using Icfpc2023.Utils;
using ShellProgressBar;

namespace Icfpc2023;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var random = new Random();

        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
        using var apiClient = new ApiClient(apiToken);
        var problems = await apiClient.GetProblemsDefinition();
        var problemId = 1;

        using var pBar = new ProgressBar(
            problems.Count,
            $"Processing");

        await Parallel.ForEachAsync(problems.Zip(Enumerable.Range(1, int.MaxValue))
                // .Where(i => i.Second == 17 || i.Second == 25)
                .Select(x => x),
            async (p, _) => await ProcessProblem(p.First, apiClient, p.Second, pBar));
    }

    private static async Task ProcessProblem(Problem problem, ApiClient apiClient, int problemId, ProgressBar pBar)
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

        var temperature = 1000d;
        var step = 1d;

        using var childBar = pBar.Spawn(
            (int)(temperature * step),
            $"[{problemId}] Start processing");

        var solver = new Solver(1000f, 1);
        var progress = new Progress<double>(_ => childBar.Tick());
        var score = solver.Solve(calculator, scene, listeners, musicians, progress);

        Console.WriteLine($"[{problemId}] Resulting score is {score}");

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

        pBar.Tick();
    }
}