using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using static System.Text.Encoding;

namespace CommandLineCalculator
{
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;

        public override void Run(UserConsole userConsole, Storage storage)
        {
            var _consoleWithStorage = new ConsoleWithStorage(userConsole, storage);

            var x = _consoleWithStorage.GetNextRandom();

            while (true)
            {
                var input = _consoleWithStorage.ReadLine();
                switch (input.Trim())
                {
                    case "exit":
                        return;
                    case "add":
                        Add(_consoleWithStorage);
                        break;
                    case "median":
                        Median(_consoleWithStorage);
                        break;
                    case "help":
                        Help(_consoleWithStorage);
                        break;
                    case "rand":
                        x = Random(_consoleWithStorage, x);
                        _consoleWithStorage.ChangeCurrentRandomValueTo(x);
                        break;
                    default:
                        _consoleWithStorage.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                _consoleWithStorage.CurrentCommandIsDone();
            }
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

        private class ConsoleWithStorage : UserConsole
        {
            private readonly UserConsole _console;
            private readonly Storage _storage;

            private int _currentLine = 1;
            
            private byte[] StorageBytes => _storage.Read();
            private List<string> StorageLines => UTF8.GetString(StorageBytes).Trim()
                .Split('\n').Where(line => !string.IsNullOrEmpty(line)).ToList();

            public ConsoleWithStorage(UserConsole console, Storage storage)
            {
                _console = console;
                _storage = storage;

                InitializeStorageIfEmpty();
            }

            private void InitializeStorageIfEmpty()
            {
                if (StorageBytes == null || StorageBytes.Length == 0)
                {
                    const long firstRandomValue = 420L;
                    AddToStorage(firstRandomValue);
                }
            }

            private void AddToStorage(string value)
            {
                var valueBytes = UTF8.GetBytes($"{value}\n");
                _storage.Write(StorageBytes.Concat(valueBytes).ToArray());
            }

            private void AddToStorage(long value)
            {
                AddToStorage(value.ToString(Culture));
            }

            public override string ReadLine()
            {
                string readLine;
                if (_currentLine < StorageLines.Count)
                {
                    readLine = StorageLines[_currentLine];
                }
                else
                {
                    readLine = _console.ReadLine();
                    AddToStorage(readLine);
                }

                _currentLine++;

                return readLine;
            }

            public void ChangeCurrentRandomValueTo(long nextRandomValue)
            {
                var randomValueBytes = UTF8.GetBytes($"{nextRandomValue}\n");
                var currentCommandBytes = UTF8.GetBytes(string.Join("\n", StorageLines.Skip(1)) + "\n");
                _storage.Write(randomValueBytes.Concat(currentCommandBytes).ToArray());
            }

            public override void WriteLine(string content)
            {
                //если в текущей строке ничего нет, то пишем, иначе не пишем
                if (_currentLine >= StorageLines.Count())
                {
                    _console.WriteLine(content);
                    AddToStorage(content);
                }

                _currentLine++;
            }

            private void ClearStorageAndWriteNextRandom()
            {
                var nextRandomValue = StorageLines[0];
                _storage.Write(Array.Empty<byte>());
                _storage.Write(UTF8.GetBytes($"{nextRandomValue}\n"));
            }

            public void CurrentCommandIsDone()
            {
                _currentLine = 1;
                ClearStorageAndWriteNextRandom();
            }

            public long GetNextRandom()
            {
                return System.Convert.ToInt64(StorageLines[0]);
            }
        }
    }
}