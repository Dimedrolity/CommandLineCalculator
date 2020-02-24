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

        private string[] StorageParts => StorageString.Split('\n')
            .Where(com => !string.IsNullOrEmpty(com)).ToArray();

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _storage = storage;
            _storageBytes = _storage.Read();
            while (true)
            {
                if (!string.IsNullOrEmpty(StorageString))
                {
                    var commandName = StorageParts[0];

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
                                _storage.Write(Encoding.UTF8.GetBytes("median\n"));
                                _storageBytes = storage.Read();
                                Median_New(userConsole);
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

            var args = StorageParts.Skip(1).ToList();

            var needToReadCount = argumentsCountOfThisCommand - args.Count;
            ReadFromConsoleNTimes(console, needToReadCount, args);

            console.WriteLine(args.Sum(int.Parse).ToString(Culture)); //result
            
            ClearStorages();
        }

        private void ReadFromConsoleNTimes(UserConsole console, int needToReadCount, List<string> args)
        {
            for (var i = 0; i < needToReadCount; i++)
            {
                var value = ReadNumber(console);
                args.Add(value.ToString());
                RewriteToStorageWithValue(value);
            }
        }

        private void Median_New(UserConsole console)
        {
            var args = StorageParts.Skip(1).ToList();

            int argumentsCountOfThisCommand;
            if (args.Count != 0)
            {
                argumentsCountOfThisCommand = int.Parse(args.First());
                args = args.Skip(1).ToList();
            }
            else
            {
                argumentsCountOfThisCommand = ReadNumber(console);
                RewriteToStorageWithValue(argumentsCountOfThisCommand);
            }
            
            var needToReadCount = argumentsCountOfThisCommand - args.Count;
            ReadFromConsoleNTimes(console, needToReadCount, args);


            var numbers = args.ConvertAll(int.Parse);
            var result = CalculateMedian(numbers);
            console.WriteLine(result.ToString(Culture));
            
            ClearStorages();
        }

        private void ClearStorages()
        {
            _storage.Write(Array.Empty<byte>());
            _storageBytes = Array.Empty<byte>();
        }

        private void RewriteToStorageWithValue(int value)
        {
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