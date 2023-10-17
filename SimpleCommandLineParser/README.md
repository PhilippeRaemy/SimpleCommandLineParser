# SimpleCommandLineParser
Simple and straight forward command line parser for dotnet console apps

## Project URL:
https://github.com/PhilippeRaemy/SimpleCommandLineParser

## Usage:
``` c#
namespace UsageExample
{
    using System;
    using System.Globalization;
    using SimpleCommandlineParser;

    class Program
    {
        static int _number;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            bool _mySwitch;
            string _command = "GuessWhat";
            var parser = new Parser()
                .WithHelpWriter(Console.WriteLine)
                .WithErrorWriter(Console.Error.WriteLine)
                .AddHelpSwitch()
                .AddSwitch("mySwitch", () => _mySwitch = true, "command line switch")
                .AddStringParameter("Command", a => _command = a, "The command to be executed", _command)
                .AddOptionalIntegerParameter("Number", a => _number = int.Parse(a, NumberStyles.Integer, CultureInfo.InvariantCulture),
                    "An integer parameter"
                .Run(args);
                
            parser.EchoParameters();
            Console.WriteLine(parser.GetHelp());
        }
    }
}
```

When called as   
`UsageExample --MySwitch --number=1 --command=HelloWorld`  
the output of this simple console app would be:  

```
Hello World!
 running with parameters: --myswitch --number=1 --command=HelloWorld

 usage is:
[--help]     : Get help on parameters
------------
[--myswitch] : command line switch
------------
 --command   : The command to be executed
             : Example: GuessWhat
------------
 --number    : An integer parameter
------------

C:\dev\SimpleCommandLineParser\UsageExample\bin\Release\netcoreapp3.1\UsageExample.exe (process 43588) exited with code 0.```