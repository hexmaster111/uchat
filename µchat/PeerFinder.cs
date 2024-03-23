using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;


public record ChatBroadCast(Message Message, ulong BroadCastId);

public record AckNetMsg(ulong BroadCastId);

public class PeerFinder
{
    internal IEnumerable<Peer> Peers => _foundPeers.Values;

    private const int Port = 5150;

    private readonly Dictionary<IPAddress, Peer> _foundPeers = new();

    private readonly string _userName;
    private readonly UdpClient _c;
    private readonly Task _listenrTask;
    private readonly Timer _sendTmr;


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

        _sendTmr = new(SendToEveryone);
        _sendTmr.Change(0, 5000);
    }

    private const string MagicString = "\bHP\rFF\nin\bder\b";

    private void SendToEveryone(object? _)
    {
        var data = Encoding.UTF8.GetBytes(MagicString + _userName);
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

            if (!remoteName.StartsWith(MagicString))
            {
                Debug.WriteLine($"{remoteName}@{from.Address} -- had a malformed magic string, ignoring");
                continue;
            }

            //toss data from myself
            if (meIp.AddressList.Contains(from.Address) && remoteName == _userName) continue;

            var p = (from.Address, remoteName);
            Debug.WriteLine($"{remoteName}@{from.Address}");
            if (_foundPeers.Any(x => x.Value.PeerId.Name.SequenceEqual(p.remoteName) && Equals(x.Key, p.Address)))
            {
                _foundPeers[p.Address].LastSeen = DateTime.Now;
                continue;
            }

            if (!_foundPeers.TryAdd(p.Address, new Peer
                {
                    LastSeen = DateTime.Now,
                    PeerId = new PeerId
                    {
                        Address = p.Address,
                        Name = p.remoteName,
                    }
                }))
            {
                throw new Exception("Ive found this peer already!");
            }
        }
    }
}