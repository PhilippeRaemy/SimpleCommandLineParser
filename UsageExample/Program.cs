using System;

namespace UsageExample
{
    using System.Globalization;
    using SimpleCommandlineParser;

    class Program
    {
        static int _number;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            bool _mySwitch;
            string _command;
            var parser = new Parser()
                .WithHelpWriter(Console.WriteLine)
                .WithErrorWriter(Console.Error.WriteLine)
                .AddHelpSwitch()
                .AddSwitch("mySwitch", () => _mySwitch = true, "command line switch")
                .AddStringParameter("Command", a => _command = a, "The command to be executed")
                .AddOptionalIntegerParameter("Number", a => _number = int.Parse(a, NumberStyles.Integer, CultureInfo.InvariantCulture),
                    "An integer parameter")
                .Run(args);
                
            parser.EchoParameters();
            Console.WriteLine(parser.GetHelp());
            
        }
    }
}
