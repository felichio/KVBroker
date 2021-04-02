using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

class ConnectionsInputSubscriber : InputSubscriber
{


    public ConnectionsInputSubscriber(string id) : base(id)
    {

    }


    public override async Task<string> InputEventHandler(Object sender, Payload p)
    {
        await Action.UpdateConnectionStatus(client);
        StringBuilder sb = new StringBuilder("\n\tHOST:PORT\tSTATUS");
        foreach ((string host, string port, string status) t in Action.ConnectionPool)
        {
            sb.Append($"\n\t{t.host}:{t.port}\t{t.status}");
        }
        return sb.ToString();
    }
}