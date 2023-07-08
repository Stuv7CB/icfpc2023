using Icfpc2023.Api;
using Icfpc2023.Domain;
using ShellProgressBar;

namespace Icfpc2023;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
        using var apiClient = new ApiClient(apiToken);
        var problems = await apiClient.GetProblemsDefinition();

        var render = new Render(problems.Count);
        render.setProblem(problems.ElementAt(1), 1);
        var app = new App(problems, render, apiClient);
        render.run();
    }
}