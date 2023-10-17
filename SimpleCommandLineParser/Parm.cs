using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCommandlineParser
{
    using System.Globalization;
    using Extensions;

    /// <summary>
    /// Command line parser class
    /// </summary>
    public partial class Parser
    {
        public int DisplayWidth = 132;

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

            public IEnumerable<string> ToStrings(int leftSize, int rightSize)
            {
                IEnumerable<string> ToLines(string s, int maxParagraphWidth, string header = "")
                {
                    var line = string.Empty;
                    var first = true;
                    var usableWidth = maxParagraphWidth - header.Length;
                    foreach (var word in s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string nextLine;
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            line = word;
                            while (line.Length > usableWidth)
                            {
                                var subLine = line.Substring(0, maxParagraphWidth);
                                line = line.Substring(maxParagraphWidth);
                                yield return header + subLine;
                                if (first)
                                {
                                    first = false;
                                    header = new string(' ', header.Length);
                                }
                            }

                            nextLine = line;
                        }
                        else nextLine = $"{line} {word}";
                        if (nextLine.Length > usableWidth)
                        {
                            yield return header + line.PadRight(usableWidth);
                            line = word;
                            if (first)
                            {
                                first = false;
                                header = new string(' ', header.Length);
                            }
                        }
                        else line = nextLine;
                    }

                    if (!string.IsNullOrWhiteSpace(line))
                        yield return header + line.PadRight(usableWidth);
                }

                var bodyWidth = leftSize;
                var pad = new string(' ', leftSize);

                var left = ToLines(Name, leftSize);
                var right = string.IsNullOrWhiteSpace(Help)
                    ? Enumerable.Empty<string>()
                    : ToLines(Help, rightSize - 3);
                if (!string.IsNullOrWhiteSpace(Example))
                {
                    right = right.Concat(ToLines(Example, rightSize - 3, "Example: "));
                }
                return left.ZipLongest(right, (l, r) => $"{l ?? pad} : {r ?? pad}")
                    .Append(new string('-', leftSize));
            }
        }

   }
}