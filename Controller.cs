using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public class Controller
{
    private delegate Task<string> Handler(Object o, Payload p);
    private event Handler PutInputHandler;
    private event Handler GetInputHandler;
    private event Handler DeleteInputHandler;
    private event Handler ConnectionsInputHandler;
    private event Handler QueryInputHandler;
    private event Handler UnknownInputHandler;
    private event Handler ExitInputHandler;
    private event Handler BatchedPutHandler;

    private Regex rx = new Regex("\\s*(GET|get|PUT|put|DELETE|delete|QUERY|query|CONNECTIONS|connections|EXIT|exit)\\s*(.*)");

    public Controller()
    {

    }

    public void Subscribe(InputSubscriber subscriber)
    {
        switch (subscriber)
        {
            case PutInputSubscriber s:
                PutInputHandler += s.InputEventHandler;
                break;
            case GetInputSubscriber s:
                GetInputHandler += s.InputEventHandler;
                break;
            case DeleteInputSubscriber s:
                DeleteInputHandler += s.InputEventHandler;
                break;
            case QueryInputSubscriber s:
                QueryInputHandler += s.InputEventHandler;
                break;
            case ConnectionsInputSubscriber s:
                ConnectionsInputHandler += s.InputEventHandler;
                break;
            
            case UnknownInputSubscriber s:
                UnknownInputHandler += s.InputEventHandler;
                break;
            case ExitInputSubscriber s:
                ExitInputHandler += s.InputEventHandler;
                break;
            case BatchedPutSubscriber s:
                BatchedPutHandler += s.InputEventHandler;
                break;
        }
    }

    public Task<string> OnBatchedEvent(string filename)
    {
        return BatchedPutHandler(this, new Payload{Message = filename});
    }

    public Task<string> OnInputEvent(string message)
    {
        if (message == null) return ExitInputHandler(this, new Payload());
        Match match = rx.Match(message);
        string command = match.Groups[1].Value;
        string mess = match.Groups[2].Value;
        if (command == "PUT" || command == "put")
        {
            return PutInputHandler(this, new Payload{Command = command, Message = mess});
        }
        else if (command == "GET" || command == "get")
        {
            return GetInputHandler(this, new Payload{Command = command, Message = mess});
        }
        else if (command == "QUERY" || command == "query")
        {
            return QueryInputHandler(this, new Payload{Command = command, Message = mess});
        }
        else if (command == "DELETE" || command == "delete")
        {
            return DeleteInputHandler(this, new Payload{Command = command, Message = mess});
        }
        else if (command == "CONNECTIONS" || command == "connections")
        {
            return ConnectionsInputHandler(this, new Payload());
        }
        else if (command == "EXIT" || command == "exit")
        {
            return ExitInputHandler(this, new Payload());
        }
        
        return UnknownInputHandler(this, new Payload());
    }
}