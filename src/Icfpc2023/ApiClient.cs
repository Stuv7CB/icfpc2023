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
            using StreamReader sr = new("./apitoken");
            Token = sr.ReadLine();
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
        public static async Task<String> Submit(uint problemId, Placements placements)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "submission");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            
            //using var multipartFormContent = new MultipartFormDataContent();
            var submission = new Submission{
                ProblemId = problemId,
                Contents = System.Text.Json.JsonSerializer.Serialize<Placements>(placements)
            };
            using StringContent jsonContent = new(
                System.Text.Json.JsonSerializer.Serialize(submission),
                System.Text.Encoding.UTF8,
                "application/json");
            // using var fileStreamContent = new StreamContent(File.OpenRead("./solutions/" + problemId.ToString() +".isl"));
            // fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            // multipartFormContent.Add(fileStreamContent, name: "file", fileName: problemId.ToString() +".isl");

            request.Content = jsonContent;

            using var response = await Policy
                                    .Handle<HttpRequestException>()
                                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt + 6)))
                                    .ExecuteAsync(async() =>
                                    {
                                        var response = await client.SendAsync(request);
                                        response.EnsureSuccessStatusCode();
                                        return response; 
                                    });
            return response.Content.ToString();
        }  
    }
}