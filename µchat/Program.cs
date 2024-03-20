using µchat;

string name = "Hailey";

if (args.Length >= 1)
{
    name = args[0];
}

Console.WriteLine($"Name = {name}");
Console.WriteLine("Listening for others...");
var pf = new PeerFinder(name);


void DoCli()
{
    const char cmdChar = '/';

    var k = Console.ReadKey();
    if (k.KeyChar == cmdChar)
    {
        Console.Write('\b');
        Console.WriteLine(
            """
            Commands:
                
            """);

        var cmd = k.KeyChar + Console.ReadLine();
        if (string.IsNullOrEmpty(cmd)) return;
        switch (cmd)
        {
            case "online":
                foreach (var VARIABLE in pf.Peers)
                {
                    
                }
                break;

            default:
                Console.WriteLine("Unknown Command!");
                break;
        }
    }
    else
    {
        var msg = Console.ReadLine();
        if (string.IsNullOrEmpty(msg)) return;
    }
}