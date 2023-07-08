using System.Text.Json.Serialization;

namespace Icfpc2023.Api
{
    public class Coords
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }
    public class Placements
    {
        [JsonPropertyName("placements")]
        public List<Coords> PlacementsList { get; set; }
    }
    public class Submission
    {
        [JsonPropertyName("problem_id")]
        public uint ProblemId { get; set; }

        [JsonPropertyName("contents")]
        public string Contents { get; set; }
    }
}
