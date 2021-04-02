using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class GetInputSubscriber : InputSubscriber
{
    Regex KeyRegex;

    public GetInputSubscriber(string id) : base(id)
    {
        KeyRegex = new Regex("^\"[a-zA-Z0-9]+\"$");
    }

    public override async Task<string> InputEventHandler(Object sender, Payload p)
    {
        
        await Action.UpdateConnectionStatus(client);
        int alive = Action.GetAlive().Count();
        int dead = Action.GetDead().Count();

        bool KeyFit = KeyRegex.IsMatch(p.Message);
        
        if (!KeyFit) return "----invalid key----";
        
        if (alive == 0) return "----all servers down----";

        var result = await Task.WhenAll(Action.GetAlive()
            .Select(((string host, string port, string status) t) => Action.Get(client, $"http://{t.host}:{t.port}/api/KV/", p.Message)));

        var real = result.Where(x => x != "NOT FOUND" && x != "DOWN");

        if (dead < Action.ReplicationLevel)
        {
            if (real.Count() > 0) return real.First();
            else return "NOT FOUND"; 
        }
        else
        {
            string text = "";
            if (real.Count() > 0)
            {
                text = real.First();
            }
            else
            {
                text = "NOT FOUND";
            }
            return text + "\n/> ----results maybe inaccurate----";
        }
    }
}