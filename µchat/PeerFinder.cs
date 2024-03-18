using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;

public class PeerFinder
{
    private const int Port = 5150;
    private readonly UdpClient _c;
    private readonly List<(IPAddress ip, string name)> _peers = new();
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

        _c.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
        _listenrTask = Task.Run(PeerDetectorMethod);

        _sendTmr = new(Send);
        _sendTmr.Change(0, 1500);
        _listenrTask.Wait();
    }

    private void Send(object? _)
    {
        var data = Encoding.UTF8.GetBytes(_userName);
        var send = _c.Send(data, data.Length, "255.255.255.255", Port);
        if (send <= 0) throw new Exception("Failed to send data");
        Console.WriteLine($"Sent {send} bytes");
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
            Console.WriteLine($"{remoteName}@{from.Address}");
            _peers.Add((from.Address, remoteName));
        }
    }
}