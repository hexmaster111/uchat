using µchat;

string name = "Hailey";

if (args.Length >= 1)
{
    name = args[0];
}

Console.WriteLine($"Name = {name}");

Console.WriteLine("Listening for others...");
var pf = new PeerFinder(name);