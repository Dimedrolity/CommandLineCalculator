using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CommandLineCalculator
{
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;

        private Storage _storage;

        private byte[] _storageBytes;
        private string StorageString => Encoding.UTF8.GetString(_storageBytes);

        private string[] StorageCommands => StorageString.Split('\n')
            .Where(com => !string.IsNullOrEmpty(com)).ToArray();

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _storage = storage;
            _storageBytes = _storage.Read();
            while (true)
            {
                if (!string.IsNullOrEmpty(StorageString))
                {
                    var commandName = StorageCommands[0];

                    switch (commandName)
                    {
                        case "add":
                            Add_New(userConsole);
                            break;
                        case "median":
                            Median_New(userConsole);
                            break;
                    }
                }
                else
                {
                    var x = 420L;
                    while (true)
                    {
                        var input = userConsole.ReadLine();
                        switch (input.Trim())
                        {
                            case "exit":
                                return;
                            case "add":
                                _storage.Write(Encoding.UTF8.GetBytes("add\n"));
                                _storageBytes = storage.Read();
                                Add_New(userConsole);
                                break;
                            case "median":
                                Median(userConsole);
                                break;
                            case "help":
                                Help(userConsole);
                                break;
                            case "rand":
                                x = Random(userConsole, x);
                                break;
                            default:
                                userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                                break;
                        }
                    }
                }
            }
        }

        private void Median_New(UserConsole userConsole)
        {
            throw new NotImplementedException();
        }

        private long Random(UserConsole console, long x)
        {
            const int a = 16807;
            const int m = 2147483647;

            var count = ReadNumber(console);
            for (var i = 0; i < count; i++)
            {
                console.WriteLine(x.ToString(Culture));
                x = a * x % m;
            }

            return x;
        }

        private void Add_New(UserConsole console)
        {
            const int argumentsCountOfThisCommand = 2;

            var args = StorageCommands.Skip(1).ToList();

            var needToReadCount = argumentsCountOfThisCommand - args.Count;

            for (var i = 0; i < needToReadCount; i++)
            {
                var value = ReadNumber(console);
                args.Add(value.ToString());
                RewriteToStorageWithValue(value);
            }


            console.WriteLine(args.Sum(int.Parse).ToString(Culture)); //result
            _storage.Write(Array.Empty<byte>());
            _storageBytes = Array.Empty<byte>();
        }

        private void RewriteToStorageWithValue(int value)
        {
            // StorageString
            var newStr = StorageString + $"{value}\n";
            _storage.Write(Encoding.UTF8.GetBytes(newStr));
            _storageBytes = _storage.Read();
        }

        private void Add(UserConsole console)
        {
            var a = ReadNumber(console);
            var b = ReadNumber(console);
            console.WriteLine((a + b).ToString(Culture));
        }

        private void Median(UserConsole console)
        {
            var count = ReadNumber(console);
            var numbers = new List<int>();
            for (var i = 0; i < count; i++)
            {
                numbers.Add(ReadNumber(console));
            }

            var result = CalculateMedian(numbers);
            console.WriteLine(result.ToString(Culture));
        }

        private double CalculateMedian(List<int> numbers)
        {
            numbers.Sort();
            var count = numbers.Count;
            if (count == 0)
                return 0;

            if (count % 2 == 1)
                return numbers[count / 2];

            return (numbers[count / 2 - 1] + numbers[count / 2]) / 2.0;
        }

        private static void Help(UserConsole console)
        {
            const string exitMessage = "Чтобы выйти из режима помощи введите end";
            const string commands = "Доступные команды: add, median, rand";

            console.WriteLine("Укажите команду, для которой хотите посмотреть помощь");
            console.WriteLine(commands);
            console.WriteLine(exitMessage);
            while (true)
            {
                var command = console.ReadLine();
                switch (command.Trim())
                {
                    case "end":
                        return;
                    case "add":
                        console.WriteLine("Вычисляет сумму двух чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "median":
                        console.WriteLine("Вычисляет медиану списка чисел");
                        console.WriteLine(exitMessage);
                        break;
                    case "rand":
                        console.WriteLine("Генерирует список случайных чисел");
                        console.WriteLine(exitMessage);
                        break;
                    default:
                        console.WriteLine("Такой команды нет");
                        console.WriteLine(commands);
                        console.WriteLine(exitMessage);
                        break;
                }
            }
        }

        private int ReadNumber(UserConsole console)
        {
            return int.Parse(console.ReadLine().Trim(), Culture);
        }
    }
}