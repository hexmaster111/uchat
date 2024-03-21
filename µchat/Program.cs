using µchat;

string name = "Hailey";

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


void DoCli()
{
    const char cmdChar = '/';
    Console.Write('>');
    var k = Console.ReadKey();
    if (k.KeyChar == cmdChar)
    {
        Console.Write('\b');
        Console.WriteLine(
            """
            Commands:
            (O)nline -- List the users who we have seen.
            """);

        var cmd = Console.ReadKey();
        Console.WriteLine();

        switch (cmd.Key)
        {
            case ConsoleKey.O:
                foreach (var p in pf.Peers)
                {
                    Console.WriteLine(p.Name + " @ " + p.LastSeenAddress + "  [ " + p.LastSeenTime + " ]");
                }

                break;

            case ConsoleKey.Escape:
            case ConsoleKey.Enter:
            default:
                Console.WriteLine("Unknown Command!");
                break;
        }
    }
    else
    {
        var msg = k.KeyChar + Console.ReadLine();
        if (string.IsNullOrEmpty(msg)) return;
    }
}