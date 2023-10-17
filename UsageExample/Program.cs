namespace UsageExample
{
    using System;
    using System.Globalization;
    using SimpleCommandlineParser;

    class Program
    {
        static int _number;

        const string LOREM_IPSUM =
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam vel diam enim. Integer gravida dolor non justo feugiat rhoncus. Aliquam id lectus viverra, faucibus sem in, luctus ligula. Etiam vestibulum hendrerit placerat. Maecenas tincidunt aliquam leo a placerat. Sed sed gravida metus, nec euismod leo. Aliquam a ipsum magna. Nulla ac risus porttitor, porta elit nec, finibus felis. Nullam id condimentum ligula. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nam in nisi tellus. Etiam elit ipsum, rhoncus a porttitor ut, euismod euismod metus. Nunc laoreet id nunc id viverra. Praesent pharetra faucibus libero, eu sollicitudin eros auctor quis.
And a new line
And another one";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            bool _mySwitch;
            string _command = "Guess what";
            var parser = new Parser()
                .WithHelpWriter(Console.WriteLine)
                .WithErrorWriter(Console.Error.WriteLine)
                .AddHelpSwitch()
                .AddSwitch("mySwitch", () => _mySwitch = true, "command line switch")
                .AddStringParameter("Command", a => _command = a, "The command to be executed", _command)
                .AddOptionalIntegerParameter("Number", a => _number = int.Parse(a, NumberStyles.Integer, CultureInfo.InvariantCulture),
                    "An integer parameter, with a very long help text... " + LOREM_IPSUM)
                .Run(args);
                
            parser.EchoParameters();
            Console.WriteLine(parser.GetHelp());
            
        }
    }
}
