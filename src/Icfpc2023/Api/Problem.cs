using System.Text.Json.Serialization;

namespace Icfpc2023.Api
{
    public class AttendeesDescription
    {
        [JsonPropertyName("x")]
        public float X { get; init; }

        [JsonPropertyName("y")]
        public float Y { get; init; }

        [JsonPropertyName("tastes")]
        public IReadOnlyCollection<float> Tastes { get; init; }
    }
    public class Problem
    {
        [JsonPropertyName("room_width")]
        public float RoomWidth { get; init; }

        [JsonPropertyName("room_height")]
        public float RoomHeight { get; init; }

        [JsonPropertyName("stage_width")]
        public float StageWidth { get; init; }

        [JsonPropertyName("stage_height")]
        public float StageHeight { get; init; }

        [JsonPropertyName("stage_bottom_left")]
        public IReadOnlyCollection<float> StageBottomLeft { get; init; }

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
