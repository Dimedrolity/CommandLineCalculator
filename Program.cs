using System;
using CommandLineCalculator.Tests;

namespace CommandLineCalculator
{
    public static class Program
    {
        public static void Main()
        {
            var console = new TextUserConsole(Console.In, Console.Out);
            var storage = new FileStorage("TestForRunExitRun");
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
            interpreter.Run(console, storage);
        }
    }
}