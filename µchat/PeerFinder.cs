using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

public class Peer
{
    public PeerId PeerId;
    public DateTime LastSeen;
}

public class PeerFinder
{
    private const int Port = 5150;
    private readonly UdpClient _c;
    private readonly Dictionary<IPAddress, Peer> _peers = new();
    private readonly Task _listenrTask;
    private readonly string _userName = "";
    private readonly Timer _sendTmr;
    internal IEnumerable<Peer> Peers => _peers.Values;


    public PeerFinder(string name)
    {
        _userName = name;
        _c = new()
        {
            ExclusiveAddressUse = false,
            MulticastLoopback = false
        };

        _c.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.36"), Port));
        _listenrTask = Task.Run(PeerDetectorMethod);

        _sendTmr = new(Send);
        _sendTmr.Change(0, 5000);
    }


    private void Send(object? _)
    {
        var data = Encoding.UTF8.GetBytes(_userName);
        var send = _c.Send(data, data.Length, "255.255.255.255", Port);
        if (send <= 0) throw new Exception("Failed to send data");
        Debug.WriteLine($"Sent {send} bytes");
    }

    private Task? PeerDetectorMethod()
    {
        var from = new IPEndPoint(0, 0);
        var me = Dns.GetHostName();
        var meIp = Dns.GetHostEntry(me);

        while (true)
        {
            var recBuff = _c.Receive(ref from);
            var remoteName = Encoding.UTF8.GetString(recBuff);
            //toss data from myself
            if (meIp.AddressList.Contains(from.Address) && remoteName == _userName) continue;

            var p = (from.Address, remoteName);
            Debug.WriteLine($"{remoteName}@{from.Address}");
            if (_peers.Any(x => x.Value.PeerId.Name.SequenceEqual(p.remoteName) && Equals(x.Key, p.Address)))
            {
                _peers[p.Address].LastSeen = DateTime.Now;
                continue;
            }

            _peers.Add(p.Address, new Peer
            {
                LastSeen = DateTime.Now,
                PeerId = new PeerId
                {
                    Address = p.Address,
                    Name = p.remoteName,
                }
            });
        }
    }
}