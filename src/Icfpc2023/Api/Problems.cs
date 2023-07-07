using System.Text.Json.Serialization;

namespace Icfpc2023.Api
{
    public class Problems
    {
        [JsonPropertyName("number_of_problems")]
        public uint numberOfProblems { get; init; }
    }
}
