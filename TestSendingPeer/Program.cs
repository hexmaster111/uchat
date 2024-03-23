// See https://aka.ms/new-console-template for more information

using System.Net;
using µchat;

var trans = new P2PTransmission(new PeerId()
{
    Address = IPAddress.Parse("192.168.1.102"),
    Name = "Thinkpad"
});

Console.WriteLine("END OF PROGRAM");