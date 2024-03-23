using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;

public class P2PTransmission
{
    private const int P2PPort = 5250;
    private const string MagicStringStart = "µchat-hps-ms-";

    private int _port;

    /// <summary>
    ///     Construct a new P2PTransmission listener
    /// </summary>
    public P2PTransmission()
    {
        retry:
        var endpoint = new IPEndPoint(IPAddress.Any, P2PPort);
        var udpClient = new UdpClient();
        var data = udpClient.Receive(ref endpoint);
        var dataStr = Encoding.UTF32.GetString(data);
        if (!dataStr.StartsWith(MagicStringStart))
        {
            udpClient.Close();
            udpClient.Dispose();
            goto retry;
        }
        // udpClient.Client.SendTo(send,endpoint);
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