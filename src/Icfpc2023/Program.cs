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

        var thread = new Thread(() => Calculate(problemChannel.Writer, solutionChannel.Writer, apiClient, problems, cancellationTokenSource.Token));
        thread.Start();

        var render = new Render(problems.Count);
        render.Run(problemChannel.Reader, solutionChannel.Reader);
        cancellationTokenSource.Cancel();
        thread.Join();
    }

    private static async Task Calculate(ChannelWriter<(int, Problem)> problemWriter, ChannelWriter<Placements> solutionWriter, ApiClient apiClient, IReadOnlyCollection<Problem> problems, CancellationToken cancellationToken)
    {
        using var app = new App(problems, apiClient);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await app.Calculate(problemWriter, solutionWriter);
        }
    }
}