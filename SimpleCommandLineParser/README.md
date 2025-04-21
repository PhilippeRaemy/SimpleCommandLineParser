# SimpleCommandLineParser
Simple and straight forward command line parser for dotnet console apps.  
Version 1.4.1 includes a refreshed list of target frameworks, and removes dependencies on a number of other libraries.  
## Breaking changes:
As of version 1.4.0...
* the constructor wants an application name and description as parameters
* the lambdas passed to the parameter methods are now Action<T> instead of Action<string>, T being the type of the parameter. This allows for better type checking and less casting.

## Bug fixes: 
Handle optional parameters.

## Usage:
``` c#
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
```

When called as   
`UsageExample --MySwitch --number=1 --command=HelloWorld --decimal=3.14 --date=1961-12-28 --datetime=1961-12-28T18:00`  
the output of this simple console app would be:  

```
Hello World!
Got a switch called mySwitch
Command parameter: HelloWorld
Got a Number argument:1
Got a Number argument:3.14
Got a Date argument:1961-12-28
Got a DateTime argument:1961-12-28T18:00:00.0000000
SimpleCommandlineParser running with parameters: --myswitch --number=1 --command=HelloWorld --decimal=3.14 --date=1961-12-28 --datetime=1961-12-28T18:00
SimpleCommandlineParser usage example
SimpleCommandlineParser usage is:
[--help]             : Get help on parameters
[--myswitch]         : command line switch
 --command           : The command to be executed
[--optionalstring]   : An optional String argument
 --number            : An integer argument
[--optionalnumber]   : An optional integer argument
 --decimal           : An Decimal argument
[--optionaldecimal]  : An optional Decimal argument
 --date              : A date argument
[--optionaldate]     : An optional date argument
 --datetime          : A DateTime argument
[--optionaldatetime] : An optional DateTime argument
```