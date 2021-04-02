using System;
using System.Threading.Tasks;

class UnknownInputSubscriber : InputSubscriber
{

    public UnknownInputSubscriber(string id) : base(id)
    {

    }

    public override Task<string> InputEventHandler(Object sender, Payload p)
    {
        return Task.FromResult(@"Unknown Command");
    }
}