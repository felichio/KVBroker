using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

public abstract class InputSubscriber
{
    private readonly string _id;
    protected HttpClient client = new HttpClient();
    
    public InputSubscriber(string id)
    {
        _id = id;
    }

    public abstract Task<string> InputEventHandler(Object sender, Payload p);
    
}