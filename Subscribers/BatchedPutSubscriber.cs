using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;


class BatchedPutSubscriber : InputSubscriber
{

    Random rng;

    public BatchedPutSubscriber(string id) : base(id)
    {
        rng = new Random();
    }


    public override async Task<string> InputEventHandler(Object sender, Payload p)
    {
        await Action.UpdateConnectionStatus(client);
        
        try
        {
            string[] lines = File.ReadAllLines(p.Message);
            List<string> list = new List<string>();
            
            int alive = Action.GetAlive().Count();
            if (alive != Action.ConnectionPool.Count())
            {
                return "Indexing must be performed with all servers in UP state. Issue CONNECTIONS to view status. Initiating without index file...";
            }
            for (int i = 0; i < lines.Length; i++)
            {
                
                var results = await Task.WhenAll(Action.GetAlive().OrderBy(x => rng.Next()).Take(Action.ReplicationLevel)
                        .Select(((string host, string port, string status) t) => Action.Put(client, $"http://{t.host}:{t.port}/api/KV/", lines[i])));
                list.Add(results.First());
            }

            // var result = await Task.WhenAll(lines.SelectMany(line => 
            //     {
            //         return Action.GetAlive().OrderBy(x => rng.Next()).Take(Action.ReplicationLevel)
            //                 .Select(((string host, string port, string status) t) => Action.Put(client, $"http://{t.host}:{t.port}/api/KV/", line));
            //     }));
            
            return list.Where(x => x == "OK").Count() + " items indexed";
        }
        catch (Exception e)
        {
            return "Invalid index file template. " + e.Message + " Initiating without index file...";
        }
        
        
    }
}