using System;
using System.Collections.Generic;
using System.Linq;
using Mannex;
using Mannex.Collections.Generic;
using MoreLinq;

namespace SimpleCommandlineParser
{
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Command line parser class
    /// </summary>
    public class Parser
    {

        /// <summary>
        /// An internal class to represent individual parameters
        /// </summary>
        public class Parm
        {
            string _name;

            /// <summary>
            /// The parameter name. Names are case insensitive.
            /// </summary>
            public string Name
            {
                get => _name;
                set => _name = value.ToLowerInvariant();
            }
            /// <summary>
            /// The help text to be displayed for this parameter
            /// </summary>
            public string Help { get; set; }
            /// <summary>
            /// A value or usage example
            /// </summary>
            public string Example { get; set; }
            /// <summary>
            /// A lambda function to be invoked with the parameter value when the parameter is found on the command line
            /// </summary>
            public Action<string> Lambda { get; set; }
            /// <summary>
            /// An action to be invoked with the parameter value when the switch is found on the command line
            /// </summary>
            public Action Action { get; set; }
            /// <summary>
            /// indicates if the parameter is optional or not. Default is not optional.
            /// </summary>
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
                var format = string.Format(Optional 
                        ? "[--{{0}}{{2}}{{3}}]{{4,{0}}}: {{1}}" 
                        : " --{{0}}{{2}}{{3}} {{4,{0}}}: {{1}}", 
                    maxlen + 1 - Name.Length - (Example == null ? 0 : Example.Length + 1)
                    );
                return string.Format(format, Name, Help,
                    Example == null ? string.Empty : "=",
                    Example ?? string.Empty,
                    string.Empty);
            }
        }

        public List<Parm> Parms { get; } = new List<Parm>();

        public void AddRange(IEnumerable<Parm> p) => Parms.AddRange(p);

        Parser AddParameter(string name, Action<string> lambda, string help, string example, bool optional)
        {
            Parms.Add(new Parm
            {
                Lambda = lambda, 
                Name = name, 
                Example = example, 
                Help = help
            });
            return this;
        }

        /// <summary>
        /// Add a string parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddStringParameter(string name, Action<string> lambda, string help, string example = null)
            => AddParameter(name, lambda, help, example, false);

        /// <summary>
        /// Add an optional string parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddOptionalStringParameter(string name, Action<string> lambda, string help, string example = null)
            => AddParameter(name, lambda, help, example, true);

        Parser AddParsedParameter(
            string name, 
            Action<string> lambda, 
            Func<string, string> parser, 
            string expected, 
            string help,
            string example, 
            bool optional)
        {
            return AddParameter(name, x =>
            {
                string parsed;
                try
                {
                    parsed = (parser(x));
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Error parsing argument `{name}`. Expecting {expected}, got value `{x}`.", e);
                }

                lambda(parsed);
            }, help, example, false);

        }

        /// <summary>
        /// Add an integer parameter. An exception is thrown if the value could not be parsed to an invariant culture integer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public Parser AddIntegerParameter(string name, Action<string> lambda, string help, string example = null, bool optional = false)
            => AddParsedParameter(name,
                lambda,
                x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                "integer",
                help,
                example,
                optional);

        /// <summary>
        /// Add an optional integer parameter. An exception is thrown if the value could not be parsed to an invariant culture integer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddOptionalIntegerParameter(string name, Action<string> lambda, string help, string example = null)
            => AddIntegerParameter(name, lambda, help, example, true);

        /// <summary>
        /// Add a decimal number parameter.
        /// An exception is thrown if the value could not be parsed to an invariant culture decimal (with a decimal dot).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public Parser AddDecimalParameter(string name, Action<string> lambda, string help, string example = null, bool optional = false)
            => AddParsedParameter(name,
                lambda,
                x => decimal.Parse(x, NumberStyles.Any, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                "decimal",
                help,
                example,
                optional);

        /// <summary>
        /// Add an optional decimal number parameter.
        /// An exception is thrown if the value could not be parsed to an invariant culture decimal (with a decimal dot).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddOptionalDecimalParameter(string name, Action<string> lambda, string help, string example = null)
            => AddDecimalParameter(name, lambda, help, example, true);

        /// <summary>
        /// Add a date parameter. An exception is thrown if the value could not be parsed to an iso date (yyyy-MM-dd).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public Parser AddDateParameter(string name, Action<string> lambda, string help, string example = null, bool optional = false)
            => AddParsedParameter(name,
                lambda,
                x => DateTime.ParseExact(x, 
                    "yyyy-MM-dd", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                "date (yyyy-MM-dd)",
                help,
                example,
                optional);

        /// <summary>
        /// Add an optional date parameter. An exception is thrown if the value could not be parsed to an iso date (yyyy-MM-dd).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddOptionalDateParameter(string name, Action<string> lambda, string help, string example = null)
            => AddDateParameter(name, lambda, help, example, true);

        /// <summary>
        /// Add a date-time parameter.
        /// An exception is thrown if the value could not be parsed to an iso date-time (yyyy-MM-dd HH:mm:ss.fffff).
        /// The seconds and fractions are optional
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public Parser AddDateTimeParameter(string name, Action<string> lambda, string help, string example = null, bool optional = false)
            => AddParsedParameter(name,
                lambda,
                x => DateTime.ParseExact(x, 
                    new[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fffff" }, 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None)
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                "DateTime (yyyy-MM-dd HH:mm:ss.fffff)",
                help,
                example,
                optional);

        /// <summary>
        /// Add an optional date-time parameter.
        /// An exception is thrown if the value could not be parsed to an iso date-time (yyyy-MM-dd HH:mm:ss.fffff).
        /// The seconds and fractions are optional
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lambda"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public Parser AddOptionalDateTimeParameter(string name, Action<string> lambda, string help, string example = null)
            => AddDateParameter(name, lambda, help, example, true);

        /// <summary>
        /// Add a switch parameter
        /// Switch parameters are by essence optional
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="help"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public Parser AddSwitch(string name, Action action, string help, string example = null)
        {
            Parms.Add(new Parm {Action = action, Name = name, Example = example, Help = help, Optional = true});
            return this;
        }

        /// <summary>
        /// Add a help switch parameter with standard text and behavior (--help)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Parser AddHelpSwitch(string name = "help")
            => AddSwitch(name, () => HelpWriter?.Invoke(GetHelp()), "Get help on parameters");

        /// <summary>
        /// Add a help writer action
        /// For instance Console.Out.WriteLine can do the job... 
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public Parser WithHelpWriter(Action<string> writer)
        {
            HelpWriter = writer;
            return this;
        }

        /// <summary>
        /// Add an error writer action
        /// For instance Console.Error.WriteLine can do the job... 
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public Parser WithErrorWriter(Action<string> writer)
        {
            ErrorWriter = writer;
            return this;
        }

        /// <summary>
        /// Parse the command line and invokes the appropriate lambda functions and switch actions.
        /// </summary>
        /// <param name="args">The array of arguments receive on the command line</param>
        /// <returns></returns>
        public Parser Run(IEnumerable<string> args)
        {
            ParseParameters(args);
            RunLambdas();
            return this;
        }

        public Action<string> HelpWriter = null;
        public Action<string> ErrorWriter = null;


        public static IEnumerable<KeyValuePair<string, string>> Parsed { get; private set; }

        public bool IsValid { get; private set; }

        public string ApplicationName { get; set; }
        public string ApplicationDescription { get; set; }

        /// <summary>
        /// Returns the help text as a printable string
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Eventually parse each argument on the command line.
        /// Switches are expected in the form `--switch`
        /// Named parameters are expected in the form `--name=value`
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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
            foreach (var parm in Parms) parm.SetDistinctiveName(Parms);

            if (hWriter == null)
                return;

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