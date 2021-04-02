using System;
using System.Threading.Tasks;

class ExitInputSubscriber : InputSubscriber
{

    public ExitInputSubscriber(string id) : base(id)
    {

    }

    public override Task<string> InputEventHandler(Object sender, Payload p)
    {   
        Console.WriteLine(@"/> " + "Exiting...");
        Environment.Exit(1);
        return Task.FromResult("Exiting...");
    }
}