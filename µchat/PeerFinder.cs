using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;

class Peer
{
    public string Name;
    public DateTime LastSeenTime;
    public IPAddress LastSeenAddress;
}

public class PeerFinder
{
    private const int Port = 5150;
    private readonly UdpClient _c;
    private readonly Dictionary<IPAddress, Peer> _peers = new();
    private readonly Task _listenrTask;
    private readonly string _userName = "";
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
            if (_peers.Any(x => x.Value.Name.SequenceEqual(p.remoteName) && Equals(x.Key, p.Address)))
            {
                _peers[p.Address].LastSeenTime = DateTime.Now;
                continue;
            }

            _peers.Add(p.Address, new Peer()
            {
                LastSeenTime = DateTime.Now,
                LastSeenAddress = p.Address,
                Name = p.remoteName,
            });
        }
    }
}