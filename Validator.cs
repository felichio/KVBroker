using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class Validator
{
    public delegate bool Pred(string x);

    private Pred predicate;
    private Regex rgx = new Regex("\\s*\"[a-zA-Z0-9]+\"\\s*[:]\\s*([{]|\"|\\d)");
    private Regex toprgx = new Regex("^\\s*\"[a-zA-Z0-9]+\"\\s*[:]\\s*[{]");

    public Validator()
    {
        predicate = x =>
        {
            try
            {
                JObject token = JObject.Parse("{" + string.Join("", x.Select(y => y == ';' ? ',' : y).ToArray()) + "}");
                
                if (token.Properties().Count() > 1)
                {
                    return false;
                }
                if (JsonNumberOfKeys(token) != RegexNumberOfKeys(x))
                {
                    return false;
                }
                
                if (!TopLevelCheck(x))
                {
                    return false;
                }

                
                
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        };
    }

    private int RegexNumberOfKeys(string test)
    {
        return rgx.Matches(test).Count;
    }

    private bool TopLevelCheck(string test)
    {
        return toprgx.IsMatch(test);
    }

    private int JsonNumberOfKeys(JToken token)
    {
        return GetKeys(token);
    }

    private int GetKeys(JToken t)
    {
        if (t is JObject o)
        {
            int s = 0;
            foreach (JProperty p in o.Properties())
            {
                s += (1 + GetKeys(p.Value));
            }
            return s;
        }
        return 0;
    }

    public Validator(Pred p)
    {
        predicate = p;
    }

    public bool validate(string test)
    {
        return predicate(test);
    }
}