﻿using System;
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
            string _name;

            public string Name
            {
                get => _name;
                set => _name = value.ToLowerInvariant();
            }

            public string Help { get; set; }
            public string Example { get; set; }
            public Action<string> Lambda { get; set; }
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

        public List<Parm> Parms { get; } = new List<Parm>();

        public void AddRange(IEnumerable<Parm> p)
        {
            Parms.AddRange(p);
        }

        public Action<string> HelpWriter = null;
        public Action<string> ErrorWriter = null;


        public static IEnumerable<KeyValuePair<string, string>> Parsed { get; private set; }

        public bool IsValid { get; private set; }

        public string ApplicationName { get; set; }
        public string ApplicationDescription { get; set; }

        public string GetHelp()
        {
            var maxlen = Parms.Max(p => p.Name.Length + (p.Example?.Length + 1 ?? 0));

            return new[]
                {
                    ApplicationDescription,
                    $"{ApplicationName} usage is:"
                }
                .Concat(Parms.Select(p => p.ToString(maxlen)))
                .ToDelimitedString(Environment.NewLine);
        }

        public IEnumerable<KeyValuePair<string, string>> ParseParameters(IEnumerable<string> args)
        {

            Parsed = args.Select(a => a.HasPrefix("--", StringComparison.Ordinal)
                                          ? a.Split('=', (name, value) => name.Substring(2).ToLowerInvariant().AsKeyTo(value))
                                          : string.Empty.AsKeyTo(a))
                         .ToList(); // anonymous
            Analyze(Parsed, HelpWriter, ErrorWriter);
            return Parsed;
        }

        void Analyze(IEnumerable<KeyValuePair<string, string>> parsed, Action<string> hWriter, Action<string> eWriter)
        {
            foreach (var parm in Parms)
            {
                parm.SetDistinctiveName(Parms);
            }

            if (hWriter == null)
            {
                return;
            }

            var missing = Parms.Where(p => !p.Optional && Parsed.All(q => !q.Key.StartsWith(p.DistinctiveName)))
                                .Select(p => p.Name).ToList();
            var illegal = Parsed.Where(q => Parms.All(p => !q.Key.StartsWith(p.DistinctiveName)))
                                .Select(q => q.Key).ToList();
            var help = Parsed.Any(p => p.Key == "?" || p.Key == "help");
            IsValid = !(help || missing.Any() || illegal.Any());
            if (!IsValid)
            {
                new[]
                    {
                        help ? string.Empty : $"{ApplicationName} was started with invalid command line options:",
                        help ? string.Empty : Labelize("Missing options: ", missing),
                        help ? string.Empty : Labelize("Illegal options: ", illegal),
                        GetHelp()
                    }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(help ? hWriter : eWriter);
            }
        }

        static string Labelize(string label, List<string> s)
        {
            if (!s.Any()) return null;
            return label + s.ToDelimitedString(", ");
        }

        public void RunLambdas()
        {
            Parms.Join(Parsed, p => p.Name, q => q.Key, (p, q) => new { p.Lambda, p.Action, q.Value })
                  .Where(x => x.Lambda != null || x.Action != null)
                  .Select(x => (Action)(() =>
                      {
                          x.Action?.Invoke();
                          x.Lambda?.Invoke(x.Value);
                      }))
                  .ForEach(λ => λ());
        }

        public void EchoParameters()
        {
            HelpWriter(
                $"{ApplicationName} running with parameters: {Parsed.Select(p => $"--{p.Key}{(string.IsNullOrWhiteSpace(p.Value) ? string.Empty : "=")}{p.Value}").ToDelimitedString(" ")}");
        }
    }
}