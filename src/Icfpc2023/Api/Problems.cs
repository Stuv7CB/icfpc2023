using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace icfpc2023.Api
{
    public class Problems
    {
        [JsonPropertyName("number_of_problems")]
        public uint numberOfProblems { get; init; }
    }
}
