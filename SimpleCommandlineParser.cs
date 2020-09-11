using System;
using System.Collections.Generic;
using System.Linq;
using Mannex;
using Mannex.Collections.Generic;
using MoreLinq;

namespace SimpleCommandlineParser
{
    public class Parser
    {

        public class Parm
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set { _name = value.ToLowerInvariant(); }
            }

            public string Help { get; set; }
            public string Example { get; set; }
            public Action<string> Lamda { get; set; }
            public Action Action { get; set; }
            public bool Optional { get; set; }

            protected internal string DistinctiveName { get; private set; }

            protected internal void SetDistinctiveName(List<Parm> others)
            {
                others = others.Except(new[] { this }).ToList();
                for (var i = 1; i < _name.Length; i++)
                {
                    if (others.Any(p => p.Name.StartsWith(_name.Substring(0, i)))) continue;
                    DistinctiveName = _name.Substring(0, i);
                    return;
                }
                DistinctiveName = Name;
            }


            public string ToString(int maxlen)
            {
                var format = string.Format(Optional ? "[--{{0}}{{2}}{{3}}]{{4,{0}}}: {{1}}" : "--{{0}}{{2}}{{3}}{{4,{0}}}: {{1}}", maxlen + 1 - Name.Length - (Example == null ? 0 : Example.Length + 1));
                return string.Format(format, Name, Help,
                    Example == null ? string.Empty : "=",
                    Example ?? string.Empty,
                    string.Empty);
            }
        }

        private readonly List<Parm> _parms = new List<Parm>();

        public List<Parm> Parms
        {
            get { return _parms; }
        }

        public void AddRange(IEnumerable<Parm> p)
        {
            _parms.AddRange(p);
        }

        public Action<string> HelpWriter = null;
        public Action<string> ErrorWriter = null;


        public static IEnumerable<KeyValuePair<string, string>> Parsed { get; private set; }

        public bool IsValid { get; private set; }

        public string ApplicationName { get; set; }
        public string ApplicationDescription { get; set; }

        public string GetHelp()
        {
            var maxlen = _parms.Max(p => p.Name.Length + (p.Example == null ? 0 : p.Example.Length + 1));

            return new[]
                {
                    ApplicationDescription,
                    string.Format("{0} usage is:", ApplicationName)
                }
                .Concat(_parms.Select(p => p.ToString(maxlen)))
                .ToDelimitedString(Environment.NewLine);
        }

        public IEnumerable<KeyValuePair<string, string>> ParseParameters(IEnumerable<string> args)
        {

            Parsed = args.Select(a => a.HasPrefix("--", StringComparison.Ordinal)
                                          ? a.Split('=', (name, value) => name.Substring(2).ToLowerInvariant().AsKeyTo(value))
                                          : string.Empty.AsKeyTo(a))
                         .ToList(); // anonymous
            Analyse(Parsed, HelpWriter, ErrorWriter);
            return Parsed;
        }

        private void Analyse(IEnumerable<KeyValuePair<string, string>> parsed, Action<string> hWriter, Action<string> eWriter)
        {
            foreach (var parm in _parms)
            {
                parm.SetDistinctiveName(_parms);
            }

            if (hWriter == null)
            {
                return;
            }

            var missing = _parms.Where(p => !p.Optional && Parsed.All(q => !q.Key.StartsWith(p.DistinctiveName)))
                                .Select(p => p.Name).ToList();
            var illegal = Parsed.Where(q => _parms.All(p => !q.Key.StartsWith(p.DistinctiveName)))
                                .Select(q => q.Key).ToList();
            var help = Parsed.Any(p => p.Key == "?" || p.Key == "help");
            IsValid = !(help || missing.Any() || illegal.Any());
            if (!IsValid)
            {
                new[]
                    {
                        help ? string.Empty : string.Format("{0} was started with invalid command line options:", ApplicationName),
                        help ? string.Empty : Labelize("Missing options: ", missing),
                        help ? string.Empty : Labelize("Illegal options: ", illegal),
                        GetHelp()
                    }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(help ? hWriter : eWriter);
            }
        }

        private static string Labelize(string label, List<string> s)
        {
            if (!s.Any()) return null;
            return label + s.ToDelimitedString(", ");
        }

        public void RunLamdas()
        {
            _parms.Join(Parsed, p => p.Name, q => q.Key, (p, q) => new { p.Lamda, p.Action, q.Value })
                  .Where(x => x.Lamda != null || x.Action != null)
                  .Select(x => (Action)(() =>
                      {
                          if (x.Action != null) x.Action();
                          if (x.Lamda != null) x.Lamda(x.Value);
                      }))
                  .ForEach(λ => λ());
            /*            _parms.Select(p => new {p, c=Parsed.FirstOrDefault(c=>c.Key.StartsWith(p.DistinctiveName))}
                p.Name, q => q.Key, (p, q) => new { p.Lamda, p.Action, q.Value })
                  .Where(x => x.Lamda != null || x.Action != null)
                  .Select(x => (Action)(() =>
                      {
                          if (x.Action != null) x.Action();
                          if (x.Lamda != null) x.Lamda(x.Value);
                      }))
                  .ForEach(λ => λ());
*/
        }

        public void EchoParameters()
        {
            HelpWriter(string.Format("{0} running with parameters: {1}", ApplicationName,
                                     Parsed.Select(p => string.Format("--{0}{1}{2}", p.Key, string.IsNullOrWhiteSpace(p.Value) ? string.Empty : "=", p.Value)).ToDelimitedString(" ")));
        }
    }
}