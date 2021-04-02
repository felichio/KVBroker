using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using NDesk.Options;
using System.Collections.Generic;



class Program
{
    
    
    static void ShowHelp(OptionSet p)
    {
        Console.WriteLine ("Usage: kvBroker [Options]");
        Console.WriteLine ("Example: kvBroker -s serverFile.txt -i dataToIndex.txt -k 2");
        Console.WriteLine ();
        Console.WriteLine ("Options:");
        p.WriteOptionDescriptions (Console.Out);
    }

    public static async Task GetInputAndRaiseEvent(Controller controller)
    {
        while (true)
        {
            Console.Write(">> ");
            var text = Console.ReadLine();
            var output = await controller.OnInputEvent(text);
            Console.WriteLine(@"/> " + output);
        }
    }

    public static async Task Main(string[] args)
    {
        int k = 0;
        string ConnectionsFile = "";
        string IndexFile = "";
        bool help = false;


        Controller controller = new Controller();
        controller.Subscribe(new PutInputSubscriber("put"));
        controller.Subscribe(new GetInputSubscriber("get"));
        controller.Subscribe(new DeleteInputSubscriber("delete"));
        controller.Subscribe(new ExitInputSubscriber("exit"));
        controller.Subscribe(new UnknownInputSubscriber("unknown"));
        controller.Subscribe(new QueryInputSubscriber("query"));
        controller.Subscribe(new ConnectionsInputSubscriber("connections"));
        controller.Subscribe(new BatchedPutSubscriber("batchput"));

        OptionSet p = new OptionSet()
        {
            {"k=", "{replication} degree", (int v) => k = v},
            {"s|connections=", "connections pool {file}", v => ConnectionsFile = v},
            {"i|index=", "index {file}", v => IndexFile = v},
            {"h|help", "show this message", v => help = v != null},
        };

        List<string> extra;
        try
        {
            extra = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Write ("kvBroker: ");
            Console.WriteLine (e.Message);
            Console.WriteLine ("Try `kvBroker --help' for more information.");
            return ;
        }

        if (help || extra.Count > 0)
        {
            ShowHelp(p);
            return ;
        }

        if (ConnectionsFile == "")
        {
            Console.WriteLine("A connection pool file must be declared");
            return ;
        }

        try
        {
            string[] lines = File.ReadAllLines(ConnectionsFile);
            Action.ConnectionPool = lines.Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                        .Select(pair => (pair[0], pair[1], "UP")).ToArray();
            
        }
        catch (Exception e)
        {
            Console.WriteLine("Invalid connections file template");
            Console.WriteLine(e.Message);
            return ;
        }

        if (k < 1 || k > Action.ConnectionPool.Count())
        {
            Console.WriteLine("The replication level must be declared in range [1, #total connections]");
            return ;
        }

        Action.ReplicationLevel = k;

        if (IndexFile != "")
        {
            var res = await controller.OnBatchedEvent(IndexFile);
            Console.WriteLine(res);
        }

        await GetInputAndRaiseEvent(controller);
    }
}

