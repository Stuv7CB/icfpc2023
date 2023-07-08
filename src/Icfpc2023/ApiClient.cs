using System.Net.Http.Headers;
using System.Text.Json;
using Icfpc2023.Api;
using Polly;

namespace Icfpc2023
{
    public class ApiClient : IDisposable
    {
        private const int MaxFileSize = 2861022;

        private readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://api.icfpcontest.com/"),
        };

        private readonly string _token;

        public ApiClient(string token)
        {
            _token = token;
        }

        public async Task<List<Problem>> GetProblemsDefinition()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "problems");

            using var response = await Policy
                                    .Handle<HttpRequestException>()
                                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                    .ExecuteAsync(async() =>
                                    {
                                        var response = await _client.SendAsync(request);
                                        response.EnsureSuccessStatusCode();
                                        return response;
                                    });
            var problemsNumber = System.Text.Json.JsonSerializer.Deserialize<Problems>(response.Content.ReadAsStream());
            if (!Directory.Exists("./problems"))
            {
                Directory.CreateDirectory("./problems");
            }
            var fCount = Directory.EnumerateFiles(new string("./problems"), "*", SearchOption.TopDirectoryOnly).Count();
            var problems = new List<Problem>();
            for (var i = 1; i <= fCount; ++i)
            {
                problems.Add(JsonSerializer.Deserialize<Problem>(File.ReadAllText("./problems/" + i.ToString() + ".json")));
            }
            for (var i = 1 + fCount; i <= problemsNumber.numberOfProblems - 1; ++i)
            {
                request = new HttpRequestMessage(HttpMethod.Get, "problem?problem_id=" + i);
                using var problemResponse = await Policy
                                            .Handle<HttpRequestException>()
                                            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                            .ExecuteAsync(async() =>
                                            {
                                                var response = await _client.SendAsync(request);
                                                response.EnsureSuccessStatusCode();
                                                return response;
                                            });
                var parsedResponse = System.Text.Json.JsonSerializer.Deserialize<ProblemRequest>(problemResponse.Content.ReadAsStream());
                File.WriteAllText("./problems/" + i.ToString() + ".json", parsedResponse.Success);
                problems.Add(JsonSerializer.Deserialize<Problem>(parsedResponse.Success));
            }
            return problems;
        }
        public async Task<string> Submit(uint problemId, Placements placements)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "submission");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var submission = new Submission{
                ProblemId = problemId,
                Contents = System.Text.Json.JsonSerializer.Serialize<Placements>(placements)
            };
            using StringContent jsonContent = new(
                System.Text.Json.JsonSerializer.Serialize(submission),
                System.Text.Encoding.UTF8,
                "application/json");

            request.Content = jsonContent;

            using var response = await Policy
                                    .Handle<HttpRequestException>()
                                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                    .ExecuteAsync(async() =>
                                    {
                                        var response = await _client.SendAsync(request);
                                        response.EnsureSuccessStatusCode();
                                        return response;
                                    });
            return response.Content.ToString();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}