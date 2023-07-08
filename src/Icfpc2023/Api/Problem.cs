using System.Text.Json.Serialization;

namespace Icfpc2023.Api
{
    public class AttendeesDescription
    {
        [JsonPropertyName("x")]
        public double X { get; init; }

        [JsonPropertyName("y")]
        public double Y { get; init; }

        [JsonPropertyName("tastes")]
        public IReadOnlyCollection<double> Tastes { get; init; }
    }
    public class Problem
    {
        [JsonPropertyName("room_width")]
        public double RoomWidth { get; init; }

        [JsonPropertyName("room_height")]
        public double RoomHeight { get; init; }

        [JsonPropertyName("stage_width")]
        public double StageWidth { get; init; }

        [JsonPropertyName("stage_height")]
        public double StageHeight { get; init; }

        [JsonPropertyName("stage_bottom_left")]
        public IReadOnlyCollection<double> StageBottomLeft { get; init; }

        [JsonPropertyName("musicians")]
        public IReadOnlyCollection<uint> Musicians { get; init; }

        [JsonPropertyName("attendees")]
        public IReadOnlyCollection<AttendeesDescription> Attendees { get; init; }
    }
    public class ProblemRequest
    {
        [JsonPropertyName("Success")]
        public string Success { get; init; }

        [JsonPropertyName("Failure")]
        public string Failure { get; init; }
    }
}
