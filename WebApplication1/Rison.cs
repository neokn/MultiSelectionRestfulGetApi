using System.Dynamic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApplication1;

public class Rison
    {
        private string currentString;
        private int index = 0;
        private string next_id_rgx = @"[^-0123456789 '!:(),*@$][^ '!:(),*@$]*";
        private Dictionary<char, Func<object>> Table;
        private Dictionary<char, Func<Rison, object>> Bands = new Dictionary<char, Func<Rison, object>>()
        {
            { 't', (x) => { return true; } },
            { 'f', (x) => { return false; } },
            { 'n', (x) => { return null; } },
            { '(', (inst) => {
                var ar = new List<dynamic>();
                char c = '\0';
                while((c = inst.Next()) != ')')
                {
                    if(c == '\0') throw new Exception("unmatched !");
                    if(ar.Count > 0)
                    {
                        if(c != ',') throw new Exception("missin ,");
                    } else if (c == ',')
                        throw new Exception("extra ,");
                    else
                        --inst.index;
                    var n = inst.ReadValue();
                    if(n != null)
                    {
                        ar.Add(n);
                    }
                }
                return ar;
            } }
        };

        private Dictionary<char, Func<object>> GetTable()
        {
            return new Dictionary<char, Func<object>>()
            {
                { '!', ExclamationMark },
                { '(', Bracket },
                { '\'', Apostrophe },
                { '-', Dash }
            };
        }

        private object ExclamationMark()
        {
            var result = new object();
            var s = currentString;
            var c = s[index++];
            if (c == '\0') throw new Exception("asdasd");
            if (Bands.TryGetValue(c, out Func<Rison, object> func))
            {
                if (c == '(')
                    result = func.Invoke(this);
                else
                    result = func.Invoke(null);
            }
            else
            {
                throw new Exception("unkown literal: !" + c);
            }
            return result;
        }
        private object Bracket()
        {
            var o = new Dictionary<string, object>();
            char c;

            int count = 0;
            while ((c = Next()) != ')')
            {
                if (count > 0)
                {
                    if (c != ',') throw new Exception("missing ,");
                }
                else if (c == ',')
                    return new Exception("extra ,");
                else
                    --index;
                var k = ReadValue();
                if (k == null) return null;
                if (Next() != ':') throw new Exception("missing :");
                var v = ReadValue();
                o.Add(k.ToString(), v);
                count++;
            }
            ExpandoObject obj = o.Expando();
            return obj;
        }

        private object Apostrophe()
        {
            var s = currentString;
            var i = index;
            var start = i;
            var segments = new List<string>();
            char c = '\0';
            while ((c = s[i++]) != '\'')
            {
                if (c == '\0') throw new Exception("unmatched '");
                if (c == '!')
                {
                    if (start < i - 1)
                        segments.Add(s.Slice(start, i - 1));
                    c = s[i++];
                    if ("!'".Contains(c.ToString()))
                    {
                        segments.Add(c.ToString());
                    }
                    else
                    {
                        throw new Exception("invalid string escape: !");
                    }
                    start = i;
                }
            }
            if (start < i - 1)
                segments.Add(s.Slice(start, i - 1));
            index = i;
            return segments.Count == 1 ? segments[0] : string.Join("", segments);
        }
        private object Dash()
        {
            var s = currentString;
            var i = index;
            var start = i - 1;
            var permittedSigns = "-";
            var state = true;
            do
            {
                var c = s[i++];
                if (c == '\0') break;
                if (char.IsDigit(c))
                    continue;
                if (permittedSigns.Contains(c.ToString()))
                {
                    permittedSigns = "";
                    continue;
                }
                state = false;
            } while (state);
            index = --i;
            s = s.Slice(start, i);
            if (s.Equals("-")) throw new Exception("invalid number");
            return s;
        }

        public string Decode(string objectToDecode)
        {
            Table = GetTable();
            return Parse(objectToDecode);
        }
        
        private string Parse(string obj)
        {
            currentString = obj;
            index = 0;
            return JsonSerializer.Serialize(ReadValue());
        }
        private object ReadValue()
        {
            var nextChar = Next();
            var copiedNextChar = nextChar;
            if (char.IsDigit(nextChar))
            {
                copiedNextChar = '-';
            }
            if (Table.TryGetValue(copiedNextChar, out Func<object> fun))
            {
                var obj = fun.Invoke();
                return obj;
            }

            var s = currentString;
            var i = index - 1;
            //remove from s from begin to i
            var removed = s.Remove(0, i);
            var matchedRgx = Regex.Match(removed, next_id_rgx);
            if (matchedRgx.Success)
            {
                var id = matchedRgx.Value;
                index = i + id.Length;
                return id;
            }
            if (nextChar != '\0')
            {
                throw new Exception("invalid character " + nextChar);
            }
            return string.Empty;
        }

        private char Next()
        {
            char c = '\0';
            var s = currentString;
            var i = index;
            do
            {
                if (i == s.Length) return '\0';
                c = s[i++];
            } while ("".IndexOf(c) >= 0);
            index = i;
            return c;
        }
    }

    public static class Extensions
    {
        public static string Slice(this string source, int start, int end)
        {
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;
            return source.Substring(start, len);
        }

        public static ExpandoObject Expando(this IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;
            foreach (var item in dictionary)
            {
                expandoDic.Add(item);
            }
            return expando;
        }
    }