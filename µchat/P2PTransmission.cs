using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace µchat;

public class P2PTransmission : IDisposable
{
    private const int P2PPort = 5250;
    private const string MagicStringStart = "µchat-hps-ms-";

    private readonly IPEndPoint _endpoint;
    private readonly UdpClient _udpClient;

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
        _endpoint = new IPEndPoint(IPAddress.Any, P2PPort);
        _udpClient = new UdpClient();
        _udpClient.Client.Bind(_endpoint);
        var str = Encoding.UTF32.GetString(_udpClient.Receive(ref _endpoint));
        if (!str.StartsWith(MagicStringStart))
        {
            _udpClient.Close();
            _udpClient.Dispose();
            goto retry;
        }

        _udpClient.Send(new[] { (byte)MessageType.HelloConnected }, _endpoint);
    }


    /// <summary>
    ///     Start a transmission with a peer
    /// </summary>
    /// <param name="peer"></param>
    public P2PTransmission(PeerId peer)
    {
        _udpClient = new UdpClient();
        _endpoint = new IPEndPoint(peer.Address, P2PPort);
        _udpClient.Connect(_endpoint);

        var sendThread = new Thread(() =>
        {
            while (!Connected)
            {
                _udpClient.Send(Encoding.UTF32.GetBytes(MagicStringStart));
                Thread.Sleep(1000);
            }
        });
        sendThread.Start();

        var resp = _udpClient.Receive(ref _endpoint);
        var msgType = (MessageType)resp[0];
        if (msgType == MessageType.HelloConnected)
        {
            Connected = true;
            sendThread.Join();
            return;
        }

        Dispose();
        sendThread.Join();
        throw new Exception("Client gave invalid response");
    }

    public bool Connected { get; private set; }
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}