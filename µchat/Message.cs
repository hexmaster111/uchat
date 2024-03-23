namespace µchat;

public struct Message
{
    public DateTime SentTime;
    public PeerId From;
    public string Text;

    public bool Equals(Message other) => SentTime.Equals(other.SentTime) &&
                                         From.Equals(other.From) && Text == other.Text;

    public override bool Equals(object? obj) => obj is Message other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(SentTime, From, Text);
    public static bool operator ==(Message left, Message right) => left.Equals(right);
    public static bool operator !=(Message left, Message right) => !(left == right);
}