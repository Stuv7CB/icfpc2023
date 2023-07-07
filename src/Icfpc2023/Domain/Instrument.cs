namespace Icfpc2023.Utils;

public class Instrument : IEquatable<Instrument>
{
    public Instrument(uint id)
    {
        Id = id;
    }

    public uint Id { get; }

    public bool Equals(Instrument? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Instrument)obj);
    }

    public override int GetHashCode()
    {
        return (int)Id;
    }
}