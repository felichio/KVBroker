using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

class DeleteInputSubscriber : InputSubscriber
{

    Regex KeyRegex;

    public DeleteInputSubscriber(string id) : base(id)
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

        if (alive != Action.ConnectionPool.Count())
        {
            return "PUT/DELETE operations can only be propagated when all initial pool connections are UP. Issue CONNECTIONS command to view status";
        }
        else
        {
            var result = await Task.WhenAll(Action.GetAlive()
            .Select(((string host, string port, string status) t) => Action.Get(client, $"http://{t.host}:{t.port}/api/KV/", p.Message)));
            
            bool exists = result.Where(x => x != "NOT FOUND").Count() > 0 ? true : false;

            if (!exists)
            {
                return "key is not in the cache";
            }
            else
            {
                result = await Task.WhenAll(Action.GetAlive()
                    .Select(((string host, string port, string status) t) => Action.Delete(client, $"http://{t.host}:{t.port}/api/KV/", p.Message)));

                return result.First();
            }
        }
        
    }
}