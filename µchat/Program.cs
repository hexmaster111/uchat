using System.Security.Cryptography;
using µchat;

string name = "Hailey";
List<Message> messages = new();

if (args.Length >= 1)
{
    name = args[0];
}

Console.WriteLine($"Name = {name}");
Console.WriteLine("Listening for others...");
var pf = new PeerFinder(name);

while (true)
{
    try
    {
        DoCli();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}


void DoCommand(string sayLine)
{
    Console.Write('\b');
    for (var i = 0; i < sayLine.Length; i++) Console.Write('\b');

    Console.WriteLine(@"Commands:
                      (O)nline -- List the users who we have seen.
                      ");

    var cmd = Console.ReadKey();
    Console.Write('\b');
    switch (cmd.Key)
    {
        case ConsoleKey.O:
            foreach (var p in pf.Peers)
            {
                Console.WriteLine("(O)nline");
                Console.WriteLine(p.PeerId.Name + " @ " + p.PeerId.Address + "  [ " + p.LastSeen + " ]");
            }

            break;

        case ConsoleKey.Escape:
        case ConsoleKey.Enter:
            break;
        default:
            Console.WriteLine("Unknown Command!");
            break;
    }
}

void SendMessage(string msg)
{
}

void DoCli()
{
    const char cmdChar = '/';
    var sayLine = $"say : ";
    Console.Write(sayLine);
    var k = Console.ReadKey();
    if (k.KeyChar == cmdChar)
    {
        DoCommand(sayLine);
    }
    else
    {
        var msg = k.KeyChar + Console.ReadLine();
        if (string.IsNullOrEmpty(msg)) return;
        SendMessage(msg);
    }
}