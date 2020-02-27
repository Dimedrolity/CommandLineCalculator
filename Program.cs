using System;
using CommandLineCalculator.Tests;

namespace CommandLineCalculator
{
    public static class Program
    {
        public static void Main()
        {
            var console = new TextUserConsole(Console.In, Console.Out);
            var storage = new FileStorage("213");
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
        }
    }
}