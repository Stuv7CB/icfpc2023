using FluentAssertions;
using Icfpc2023.Domain;

namespace Icfpc2023.Tests;

public class MusicianTests
{
    [Fact]
    public void DoesBlock_ShouldReturnFalse_IfTooFar()
    {
        var listener = new Listener(0, new PointDto(0, 0), new Dictionary<Instrument, Taste>());

        var musician = new Musician(0, new Instrument(0));
        musician.AdjustPosition(new PointDto(0, 10));

        var otherMusician = new Musician(1, new Instrument(0));
        otherMusician.AdjustPosition(new PointDto(10, 0));

        otherMusician.DoesBlocks(musician, listener).Should().BeFalse();
    }

    [Fact]
    public void DoesBlock_ShouldReturnFalse_IfBehindFarAway()
    {
        var listener = new Listener(0, new PointDto(0, 0), new Dictionary<Instrument, Taste>());

        var musician = new Musician(0, new Instrument(0));
        musician.AdjustPosition(new PointDto(0, 10));

        var otherMusician = new Musician(1, new Instrument(0));
        otherMusician.AdjustPosition(new PointDto(0, 20));

        otherMusician.DoesBlocks(musician, listener).Should().BeFalse();
    }

    [Fact]
    public void DoesBlock_ShouldReturnTrue_IfBehindNotFarAway()
    {
        var listener = new Listener(0, new PointDto(0, 0), new Dictionary<Instrument, Taste>());

        var musician = new Musician(0, new Instrument(0));
        musician.AdjustPosition(new PointDto(0, 10));

        var otherMusician = new Musician(1, new Instrument(0));
        otherMusician.AdjustPosition(new PointDto(0, 15));

        otherMusician.DoesBlocks(musician, listener).Should().BeTrue();
    }

    [Fact]
    public void DoesBlock_ShouldReturnTrue_IfBefore()
    {
        var listener = new Listener(0, new PointDto(0, 0), new Dictionary<Instrument, Taste>());

        var musician = new Musician(0, new Instrument(0));
        musician.AdjustPosition(new PointDto(0, 10));

        var otherMusician = new Musician(1, new Instrument(0));
        otherMusician.AdjustPosition(new PointDto(0, 5));

        otherMusician.DoesBlocks(musician, listener).Should().BeTrue();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(4)]
    public void DoesBlock_ShouldReturnTrue_IfNearLine(int xCoord)
    {
        var listener = new Listener(0, new PointDto(0, 0), new Dictionary<Instrument, Taste>());

        var musician = new Musician(0, new Instrument(0));
        musician.AdjustPosition(new PointDto(0, 10));

        var otherMusician = new Musician(1, new Instrument(0));
        otherMusician.AdjustPosition(new PointDto(xCoord, 5));

        otherMusician.DoesBlocks(musician, listener).Should().BeTrue();
    }
}