// See https://aka.ms/new-console-template for more information

using System.Net;
using µchat;

try
{
    var trans = new P2PTransmission(new PeerId()
    {
        Address = IPAddress.Parse("192.168.1.102"),
        Name = "Thinkpad"
    });
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

Console.WriteLine("End of program");