using System.Threading.Channels;
using Icfpc2023.Api;

namespace Icfpc2023;

internal static class Program
{
    private static void Main(string[] args)
    {
        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
        using var apiClient = new ApiClient(apiToken);
        var problems = apiClient.GetProblemsDefinition().GetAwaiter().GetResult();

        var problemChannel = Channel.CreateUnbounded<(int, Problem)>();
        var solutionChannel = Channel.CreateUnbounded<Placements>();
        using var cancellationTokenSource = new CancellationTokenSource();

        var thread = new Thread(() => Calculate(
            problemChannel.Writer,
            solutionChannel.Writer,
            apiClient,
            problems,
            cancellationTokenSource.Token));
        thread.Start();

        var render = new Render(problems.Count);
        render.Run(problemChannel.Reader, solutionChannel.Reader);
        cancellationTokenSource.Cancel();
        thread.Join();
    }

    private static async Task Calculate(ChannelWriter<(int, Problem)> problemWriter,
        ChannelWriter<Placements> solutionWriter, ApiClient apiClient, IReadOnlyCollection<Problem> problems,
        CancellationToken cancellationToken)
    {
        using var app = new App(problems, apiClient);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine("Input R and next problem to solve or 0 for all. E.g. 'R 10'");
            Console.WriteLine("Input F and next problem for force recount.");
            Console.WriteLine("Input V and then number for setting problem for view without solving");

            var commandString = Console.ReadLine();

            if (commandString.StartsWith("V"))
            {
                var problemIndex = int.Parse(commandString[2..]);

                await problemWriter.WriteAsync((problemIndex, problems.ElementAt(problemIndex - 1)), cancellationToken);

                if (app.TryGetSolution(problemIndex, out var solution))
                {
                    await solutionWriter.WriteAsync(solution, cancellationToken);
                }

                continue;
            }

            if (commandString.StartsWith("R"))
            {
                var problemIndex = int.Parse(commandString[2..]);

                await problemWriter.WriteAsync((problemIndex, problems.ElementAt(problemIndex - 1)), cancellationToken);

                await app.Calculate(problemIndex, false);

                if (app.TryGetSolution(problemIndex, out var solution))
                {
                    await solutionWriter.WriteAsync(solution, cancellationToken);
                }
            }

            if (commandString.StartsWith("F"))
            {
                var problemIndex = int.Parse(commandString[2..]);

                await problemWriter.WriteAsync((problemIndex, problems.ElementAt(problemIndex - 1)), cancellationToken);

                await app.Calculate(problemIndex, true);

                if (app.TryGetSolution(problemIndex, out var solution))
                {
                    await solutionWriter.WriteAsync(solution, cancellationToken);
                }
            }
        }
    }
}