namespace UsageExample
{
    using System;
    using SimpleCommandlineParser;

    static class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var parser = new Parser("SimpleCommandlineParser", "SimpleCommandlineParser usage example")
                .WithHelpWriter(Console.WriteLine)
                .WithErrorWriter(Console.Error.WriteLine)
                .AddHelpSwitch()
                .AddSwitch("mySwitch", () => Console.WriteLine($"Got a switch called mySwitch"), "command line switch")
                .AddStringParameter("Command", a => Console.WriteLine($"Command parameter: {a}"), "The command to be executed")
                .AddOptionalStringParameter("OptionalString", a => Console.WriteLine($"OptionalStringArgument: {a}"), "An optional String argument")
                .AddIntegerParameter("Number", a => Console.WriteLine($"Got a Number argument:{a}"), "An integer argument")
                .AddOptionalIntegerParameter("OptionalNumber", a => Console.WriteLine($"Got an optional Number argument:{a}"), "An optional integer argument")
                .AddDecimalParameter("Decimal", a => Console.WriteLine($"Got a Number argument:{a}"), "An Decimal argument")
                .AddOptionalDecimalParameter("OptionalDecimal", a => Console.WriteLine($"Got an optional Number argument:{a}"), "An optional Decimal argument")
                .AddDateParameter("Date", a => Console.WriteLine($"Got a Date argument:{a:D}"), "A date argument")
                .AddOptionalDateParameter("OptionalDate", a => Console.WriteLine($"Got an optional Date argument:{a}"), "An optional date argument")
                .AddDateTimeParameter("DateTime", a => Console.WriteLine($"Got a DateTime argument:{a:O}"), "A DateTime argument")
                .AddOptionalDateTimeParameter("OptionalDateTime", a => Console.WriteLine($"Got an optional DateTime argument:{a}"), "An optional DateTime argument")
                .Run(args);
                
            parser.EchoParameters();
            Console.WriteLine(parser.GetHelp());
            
        }
    }
}
