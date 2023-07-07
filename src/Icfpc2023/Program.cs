namespace icfpc2023
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var problems = await ApiClient.GetProblemsDefinition();
        }
    }
}
