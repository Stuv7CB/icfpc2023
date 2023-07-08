using FluentAssertions;
using Icfpc2023.Domain;

namespace Icfpc2023.Tests;

public class ScoreCalculatorTests
{
    [Fact]
    public void CalculatorScore()
    {
        var scene = new Scene
        {
            BottomLeft = new PointDto(500, 0),
            Width = 1000,
            Height = 200,
        };

        var instruments = new[]
        {
            new Instrument(0),
            new Instrument(1)
        };

        var musicians = new[]
        {
            new Musician(0, instruments[0]),
            new Musician(1, instruments[1]),
            new Musician(2, instruments[0])
        };

        musicians[0].AdjustPosition(new PointDto(590, 10));
        musicians[1].AdjustPosition(new PointDto(1100, 100));
        musicians[2].AdjustPosition(new PointDto(1100, 150));

        var listeners = new[]
        {
            new Listener(0, new PointDto(100, 500), new Dictionary<Instrument, Taste>
            {
                [instruments[0]] = new Taste
                {
                    Instrument = instruments[0],
                    Value = 1000
                },
                [instruments[1]] = new Taste
                {
                    Instrument = instruments[1],
                    Value = -1000
                }
            }),
            new Listener(1, new PointDto(200, 1000), new Dictionary<Instrument, Taste>
            {
                [instruments[0]] = new Taste
                {
                    Instrument = instruments[0],
                    Value = 200
                },
                [instruments[1]] = new Taste
                {
                    Instrument = instruments[1],
                    Value = 200
                }
            }),
            new Listener(2, new PointDto(1100, 800), new Dictionary<Instrument, Taste>
            {
                [instruments[0]] = new Taste
                {
                    Instrument = instruments[0],
                    Value = 800
                },
                [instruments[1]] = new Taste
                {
                    Instrument = instruments[1],
                    Value = 1500
                }
            }),
        };

        var pillars = new[]
        {
            new Pillar(0, new PointDto(345, 255), 4)
        };

        var scoreCalculator = new ScoreCalculator(100, scene, listeners, pillars);
        scoreCalculator.CalculateScore(musicians).Should().Be(3270);
    }
}