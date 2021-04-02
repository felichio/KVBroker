using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;



class PutInputSubscriber : InputSubscriber
{

    Validator validator;
    Regex TopLevelKeyRegex;
    Random rng;

    public PutInputSubscriber(string id) : base(id)
    {
        validator = new Validator();
        TopLevelKeyRegex = new Regex("^\"[a-zA-Z0-9]+\"");
        rng = new Random();
    }

    public override async Task<string> InputEventHandler(Object sender, Payload p)
    {
        await Action.UpdateConnectionStatus(client);
        int alive = Action.GetAlive().Count();
        int dead = Action.GetDead().Count();

        if (validator.validate(p.Message))
        {
            
            if (alive == 0) return "----all servers down----";


            if (alive != Action.ConnectionPool.Count())
            {
                return "PUT/DELETE operations can only be propagated when all initial pool connections are UP. Issue CONNECTIONS command to view status";
            }
            else
            {
                P pair = P.FromString(p.Message);
                

                var result = await Task.WhenAll(Action.GetAlive()
                    .Select(((string host, string port, string status) t) => Action.Get(client, $"http://{t.host}:{t.port}/api/KV/", "\"" + pair.key + "\"")));

                bool exists = result.Where(x => x != "NOT FOUND").Count() > 0 ? true : false;
                
                if (exists)
                {
                    return "key already in cache";
                }
                else
                {
                    result = await Task.WhenAll(Action.GetAlive().OrderBy(x => rng.Next()).Take(Action.ReplicationLevel)
                        .Select(((string host, string port, string status) t) => Action.Put(client, $"http://{t.host}:{t.port}/api/KV/", p.Message)));
                    
                    return result.First();
                }
            }
        }
        else
        {
            return "invalid payload pattern";
        }

        
        
    }
}