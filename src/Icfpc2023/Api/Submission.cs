using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace icfpc2023.Api
{
    public class Coords
    {
        [JsonPropertyName("x")]
        public float X { get; set; }
        
        [JsonPropertyName("y")]
        public float Y { get; set; }
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
        public String Contents { get; set; }
    }
}
