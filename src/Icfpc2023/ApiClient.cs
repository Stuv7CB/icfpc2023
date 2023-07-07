using icfpc2023.Api;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace icfpc2023
{
    public static class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseAddress = "https://api.icfpcontest.com/";
        private static string Token;
        private const int MaxFileSize = 2861022;

        static ApiClient()
        {
            // using StreamReader sr = new("./apitoken");
            // Token = sr.ReadLine();
        }

        public static async Task<List<Problem>> GetProblemsDefinition()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "problems");

            using var response = await Policy
                                    .Handle<HttpRequestException>()
                                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                    .ExecuteAsync(async() =>
                                    {
                                        var response = await client.SendAsync(request);
                                        response.EnsureSuccessStatusCode();
                                        return response; 
                                    });
            var problemsNumber = System.Text.Json.JsonSerializer.Deserialize<Problems>(response.Content.ReadAsStream());
            if (!Directory.Exists("./problems"))
            {
                Directory.CreateDirectory("./problems");
            }
            var fCount = Directory.EnumerateFiles(new String("./problems"), "*", SearchOption.TopDirectoryOnly).Count();
            var problems = new List<Problem>();
            for (var i = 1; i <= fCount; ++i)
            {
                problems.Add(System.Text.Json.JsonSerializer.Deserialize<Problem>(File.ReadAllText("./problems/" + i.ToString() + ".json")));
            }
            for (var i = 1 + fCount; i <= problemsNumber.numberOfProblems; ++i)
            {
                request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "problem?problem_id=" + i.ToString());
                using var problemResponse = await Policy
                                            .Handle<HttpRequestException>()
                                            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                            .ExecuteAsync(async() =>
                                            {
                                                var response = await client.SendAsync(request);
                                                response.EnsureSuccessStatusCode();
                                                return response; 
                                            });
                var parsedResponse = System.Text.Json.JsonSerializer.Deserialize<ProblemRequest>(problemResponse.Content.ReadAsStream());
                File.WriteAllText("./problems/" + i.ToString() + ".json", parsedResponse.Success);
                problems.Add(System.Text.Json.JsonSerializer.Deserialize<Problem>(parsedResponse.Success));
            }
            return problems;
        }
    }
}