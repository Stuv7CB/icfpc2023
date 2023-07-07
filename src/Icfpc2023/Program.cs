using Icfpc2023.Utils;

namespace Icfpc2023;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
        using var apiClient = new ApiClient(apiToken);
        var problems = await apiClient.GetProblemsDefinition();

        foreach (var problem in problems)
        {
            var numberOfInstruments = problem.Musicians.Max() + 1;

            var instruments = Enumerable.Range(0, (int)numberOfInstruments)
                .Select(i => new Instrument((uint)i))
                .ToDictionary(i => i.Id);

            var musitions = problem.Musicians
                .Select((instrument, i) => new Musicion(i, instruments[instrument]))
                .ToArray();

            var listeners = problem.Attendees
                .Select((a, i) => new Listener(
                    i,
                    new PointDto
                    {
                        X = a.X,
                        Y = a.Y
                    },
                    a.Tastes.Select((t, i) => (t, i)).ToDictionary(
                        kv => instruments[(uint)kv.i],
                        kv => new Taste
                        {
                            Instrument = instruments[(uint)kv.i],
                            Value = kv.t
                        }))).ToArray();

            Console.WriteLine(listeners);
        }
    }
}