using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace µchat;

public class P2PTransmission : IDisposable
{
    private const int P2PPort = 5250;
    private const string MagicStringStart = "µchat-hps-ms-";
    private static readonly byte[] MagicBytesA = new byte[] { 0xDA, 0xDD, (byte)'Y', (byte)'S', 0xFF, (byte)'T' };
    private static readonly int MagicBytesALen = MagicBytesA.Length;

    private readonly IPEndPoint _fromEndPoint;
    private readonly UdpClient _udpClient;

    enum MessageType : byte
    {
        HelloConnected = 0x01,
    }

    /// <summary>
    ///     Construct a new P2PTransmission listener
    /// </summary>
    public P2PTransmission()
    {
        retry:
        _fromEndPoint = new IPEndPoint(IPAddress.Any, P2PPort);
        _udpClient = new UdpClient();
        _udpClient.Client.Bind(_fromEndPoint);
        var str = Encoding.UTF32.GetString(_udpClient.Receive(ref _fromEndPoint));
        if (!str.StartsWith(MagicStringStart))
        {
            _udpClient.Close();
            _udpClient.Dispose();
            goto retry;
        }

        _udpClient.Connect(_fromEndPoint);

        var ourNewPort = PortHelper.NextFreePort(P2PPort);

        var msg = new List<byte>
        {
            (byte)MessageType.HelloConnected,
        };
        msg.AddRange(MagicBytesA);
        msg.AddRange(Encoding.UTF32.GetBytes(ourNewPort.ToString()));
        _udpClient.Send(msg.ToArray());
        _udpClient.Close();
        _udpClient.Dispose();

        _udpClient = new UdpClient(ourNewPort);
        _fromEndPoint = new IPEndPoint(_fromEndPoint.Address, ourNewPort);
        _udpClient.Connect(_fromEndPoint);
    }


    /// <summary>
    ///     Start a transmission with a peer
    /// </summary>
    /// <param name="peer"></param>
    public P2PTransmission(PeerId peer)
    {
        _udpClient = new UdpClient();
        _fromEndPoint = new IPEndPoint(peer.Address, P2PPort);
        _udpClient.Connect(_fromEndPoint);

        var cts = new CancellationTokenSource();

        var sendThread = new Thread(() =>
        {
            int connectionTry = 0;
            while (!Connected && !Disposed)
            {
                _udpClient.Send(Encoding.UTF32.GetBytes(MagicStringStart));
                connectionTry++;
                if (connectionTry >= 5)
                {
                    cts.Cancel();
                    break;
                }

                Task.Delay(1000, cts.Token).Wait();
            }
        }) { IsBackground = true };


        var readTask = _udpClient.ReceiveAsync(cts.Token).AsTask();
        sendThread.Start();

        try
        {
            readTask.Wait(cts.Token);
        }
        catch (OperationCanceledException oce)
        {
            //Ignore
        }

        if (!readTask.IsCompletedSuccessfully)
        {
            Dispose();
            sendThread.Join();
            throw new Exception("Failed to connect :(");
        }

        var resp = readTask.Result.Buffer;
        var msgType = (MessageType)resp[0];
        if (msgType != MessageType.HelloConnected) ThrowInvalidResponse();

        var magicBytes = resp[1..(MagicBytesALen + 1)];
        if (!magicBytes.SequenceEqual(MagicBytesA)) ThrowInvalidResponse();

        var portStr = Encoding.UTF32.GetString(resp[(MagicBytesALen + 1)..]);
        if (!int.TryParse(portStr, out var port)) ThrowInvalidResponse();

        Connected = true;
        sendThread.Join();

        _udpClient.Close();
        _udpClient.Dispose();
        _udpClient = new UdpClient(port);
        _fromEndPoint = new IPEndPoint(_fromEndPoint.Address, port);
        _udpClient.Connect(_fromEndPoint);

        return;

        void ThrowInvalidResponse()
        {
            Dispose();
            sendThread?.Join();
            throw new Exception("Client gave invalid response");
        }
    }

    public bool Connected { get; private set; }
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
        Connected = false;
    }
}

public static class PortHelper
{
    static bool IsFree(int port)
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] listeners = properties.GetActiveTcpListeners();
        IPEndPoint[] listenersUDP = properties.GetActiveUdpListeners();
        listeners = listeners.Concat(listenersUDP).ToArray();
        int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
        return openPorts.All(openPort => openPort != port);
    }

    public static int NextFreePort(int port = 0)
    {
        port = (port > 0) ? port : new Random().Next(1, 65535);
        while (!IsFree(port))
        {
            port += 1;
        }

        return port;
    }
}