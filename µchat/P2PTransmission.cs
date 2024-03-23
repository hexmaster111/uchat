using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;

public class P2PTransmission
{
    private const int P2PPort = 5250;
    private const string MagicStringStart = "µchat-hps-ms-";

    enum MessageType : byte
    {
        HelloConnected = 0x01
    }

    /// <summary>
    ///     Construct a new P2PTransmission listener
    /// </summary>
    public P2PTransmission()
    {
        retry:
        var endpoint = new IPEndPoint(IPAddress.Any, P2PPort);
        var udpClient = new UdpClient();
        udpClient.Client.Bind(endpoint);
        var str = Encoding.UTF32.GetString(udpClient.Receive(ref endpoint));
        if (!str.StartsWith(MagicStringStart))
        {
            udpClient.Close();
            udpClient.Dispose();
            goto retry;
        }
        
        udpClient.Send(new[] { (byte)MessageType.HelloConnected }, endpoint);
    }


    /// <summary>
    ///     Start a transmission with a peer
    /// </summary>
    /// <param name="peer"></param>
    public P2PTransmission(PeerId peer)
    {
        var udpClient = new UdpClient();
        udpClient.Connect(new IPEndPoint(peer.Address, P2PPort));
        udpClient.Send(Encoding.UTF32.GetBytes(MagicStringStart));
    }
}