using System.Net;

namespace µchat;

public struct PeerId
{
    public string Name;
    public IPAddress Address;

    public override bool Equals(object? obj) => obj is PeerId id && Equals(id);
    public bool Equals(PeerId other) => Name == other.Name && Address.Equals(other.Address);
    public override int GetHashCode() => HashCode.Combine(Name, Address);

    public static bool operator ==(PeerId left, PeerId right) => left.Equals(right);

    public static bool operator !=(PeerId left, PeerId right) => !(left == right);
}