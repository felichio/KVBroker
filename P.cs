using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class P
{
    private string _key;
    private V _value;
    
    public string key 
    {
        get => _key;
        set => _key = value;
    }
    public V value 
    {
        get => _value;
        set => _value = value;
    }

    public P(string key, V value)
    {
        _key = key;
        _value = value;
    }

    public P(string key, ScalarV<string> value)
    {
        _key = key;
        _value = value;
    }

    public P(string key, ScalarV<double> value)
    {
        _key = key;
        _value = value;
    }

    public P(string key, ScalarV<int> value)
    {
        _key = key;
        _value = value;
    }

    public P(string key)
    {
        _key = key;
        _value = null;
    }

    

    public static List<(string, P)> FindKey(P p, string key, string superkey = "")
    {
        List<(string, P)> t = new List<(string, P)>();
        if (p.key == key)
        {
            t.Add(($"{(superkey == "" ? superkey : $"{superkey}.")}{key}", p));
        }

        if (p.value is CollectionV c)
        {
            foreach (P p1 in c)
            {
                t = t.Concat(FindKey(p1, key, $"{(superkey == "" ? superkey : $"{superkey}.")}{p.key}")).ToList();
            }
        }

        return t;
        
    }

    public override string ToString()
    {
        switch (_value)
        {
            case ScalarV<string> s:
                return $"\"{_key}\": \"{s.value}\"";
            case ScalarV<int> i:
                return $"\"{_key}\": {i.value.ToString()}";
            case ScalarV<double> d:
                return $"\"{_key}\": {d.value.ToString()}";
            case CollectionV c:
                StringBuilder sb = new StringBuilder($"\"{_key}\"" + ": {");
                
                foreach (P p in c)
                {
                    if (p == c.Last()) sb.Append(p.ToString());
                    else sb.Append(p.ToString() + "; ");
                }
                return sb.ToString() + "}";
            default:
                return "";
        }
    }


    public string ToJsonString()
    {
        return "{" + string.Join("", ToString().Select(x => x == ';' ? ',' : x).ToArray()) + "}";
    }

    private static P FromJson(JToken token, string key)
    {
        if (token is JObject o)
        {
            
            CollectionV v = new CollectionV();
            foreach(JProperty p in o.Properties())
            {
                
                // Console.WriteLine(p.Name);
                if (p.Value.Type == JTokenType.Integer)
                {
                    v.Add(new P(p.Name, new ScalarV<int>((int) p.Value)));
                }
                else if (p.Value.Type == JTokenType.Float)
                {
                    v.Add(new P(p.Name, new ScalarV<double>((double) p.Value)));
                }
                else if (p.Value.Type == JTokenType.String)
                {
                    v.Add(new P(p.Name, new ScalarV<string>((string) p.Value)));
                }
                else {
                    v.Add(FromJson(p.Value, p.Name));
                    
                }   
            }
            return new P(key, v);
        }
        return null;
    }

    public static P FromString(string payload)
    {
        return FromJson(JToken.Parse("{" + string.Join("", payload.Select(x => x == ';' ? ',' : x).ToArray()) + "}"));
    }

    private static P FromJson(JToken token)
    {
        return FromJson(((JObject) token).Properties().First().Value, ((JObject) token).Properties().First().Name);
    }
}

public abstract class  V
{
    
}

public class CollectionV : V, IEnumerable
{
    private Collection<P> _value;

    public CollectionV(Collection<P> value)
    {
        _value = value;
    }

    public CollectionV()
    {
        _value = new Collection<P>();
    }

    public Collection<P> value
    {
        get => _value;
        set => _value = value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _value.GetEnumerator();
    }

    public P Last()
    {
        return _value.Last();
    }

    public void Add(P p)
    {
        _value.Add(p);
    }

    public int Count()
    {
        return _value.Count;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("{");
        foreach (P p in _value)
        {
            if (p == Last()) sb.Append(p.ToString());
            else sb.Append(p.ToString() + "; ");
        }
            return sb.ToString() + "}";
    }

    
}

public class ScalarV<T> : V
{
    private T _value;

    public T value 
    {
        get => _value;
        set => _value = value;
    }

    public ScalarV(T value)
    {
        _value = value;
    }

    public static implicit operator ScalarV<T>(T t)
    {
        return new ScalarV<T>(t);
    }

    public static bool temp(int i)
    {
        return i < 10 ? true : false;
    }

    public override string ToString()
    {
        if (_value is string)
        {
            return "\"" + _value + "\"";
        }
        if (_value is int || _value is double)
        {
            return _value.ToString();
        }
        return "";
    }
}