
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

public static class Action
{
    

    

    public static (string, string, string)[] ConnectionPool {get; set;}
    public static int ReplicationLevel {get; set;}

    static Action()
    {

    }

    public static async Task<string> Put(HttpClient client, string path, string data)
    {
        try
        {
            HttpResponseMessage response = await client.PostAsync(path, new StringContent(data));
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "DOWN";
        }
        
    }

    public static async Task<string> Get(HttpClient client, string path, string data)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(path + data);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            return "DOWN";
        }
        
    }

    public static async Task<string> Delete(HttpClient client, string path, string data)
    {
        HttpResponseMessage response = await client.DeleteAsync(path + data);
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task UpdateConnectionStatus(HttpClient client)
    {
        var result = await Task.WhenAll(ConnectionPool.Select(((string host, string port, string status) t) => Get(client, $"http://{t.host}:{t.port}/api/KV/service/alive", "")));
        // Side effect, Connection pool status edited
        ConnectionPool = ConnectionPool.Select(((string host, string port, string status) t, int index) => (t.host, t.port, result[index])).ToArray();
        
    }

    public static (string, string, string)[] GetAlive()
    {
        return ConnectionPool
            .Where(((string host, string port, string status) t) => t.status == "UP")
            .ToArray();
    }

    public static (string, string, string)[] GetDead()
    {
        return ConnectionPool
            .Where(((string host, string port, string status) t) => t.status == "DOWN")
            .ToArray();
    }
}