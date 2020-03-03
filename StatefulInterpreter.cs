using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Text.Encoding;

namespace CommandLineCalculator
{
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;

        public override void Run(UserConsole userConsole, Storage storage)
        {
            var consoleWithStorage = new ConsoleWithStorage(userConsole, storage);

            var x = consoleWithStorage.GetNextRandom();

            while (true)
            {
                var input = consoleWithStorage.ReadLine();
                switch (input.Trim())
                {
                    case "exit":
                        return;
                    case "add":
                        Add(consoleWithStorage);
                        break;
                    case "median":
                        Median(consoleWithStorage);
                        break;
                    case "help":
                        Help(consoleWithStorage);
                        break;
                    case "rand":
                        x = Random(consoleWithStorage, x);
                        consoleWithStorage.ChangeCurrentRandomValueTo(x);
                        break;
                    default:
                        consoleWithStorage.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                consoleWithStorage.CurrentCommandIsDone();
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

            private byte[] _storageBytes;

            private List<string> _storageLines;

            public ConsoleWithStorage(UserConsole console, Storage storage)
            {
                _console = console;
                _storage = storage;
                _storageBytes = storage.Read();
                RewriteStorageLines();
                InitializeStorageIfEmpty();
            }

            private void RewriteStorageLines()
            {
                _storageLines = UTF8.GetString(_storageBytes).Trim()
                    .Split('\n').Where(line => !string.IsNullOrEmpty(line)).ToList();
            }

            private void InitializeStorageIfEmpty()
            {
                if (_storageBytes == null || _storageBytes.Length == 0)
                {
                    const long firstRandomValue = 420L;
                    AddToStorage(firstRandomValue);
                }
            }

            public long GetNextRandom()
            {
                return System.Convert.ToInt64(_storageLines[0]);
            }

            public void CurrentCommandIsDone()
            {
                _currentLine = 1;
                ClearStorageAndWriteNextRandom();
            }

            public override string ReadLine()
            {
                string readLine;
                if (_currentLine < _storageLines.Count)
                {
                    readLine = _storageLines[_currentLine];
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
                var currentCommandBytes = UTF8.GetBytes(string.Join("\n", _storageLines.Skip(1)) + "\n");
                var bytesWithNextRandomValue = randomValueBytes.Concat(currentCommandBytes).ToArray();
                _storage.Write(bytesWithNextRandomValue);
                _storageBytes = bytesWithNextRandomValue;
                RewriteStorageLines();
            }

            public override void WriteLine(string content)
            {
                if (_currentLine == _storageLines.Count)
                {
                    _console.WriteLine(content);
                    AddToStorage(content);
                }

                _currentLine++;
            }

            private void AddToStorage(long value)
            {
                AddToStorage(value.ToString(Culture));
            }

            private void AddToStorage(string value)
            {
                var valueBytes = UTF8.GetBytes($"{value}\n");
                var bytesWithNewValue = _storageBytes.Concat(valueBytes).ToArray();
                _storage.Write(bytesWithNewValue);
                _storageBytes = bytesWithNewValue;
                RewriteStorageLines();
            }


            private void ClearStorageAndWriteNextRandom()
            {
                var nextRandomValue = GetNextRandom();
                _storage.Write(Array.Empty<byte>());
                var nextRandomValueBytes = UTF8.GetBytes($"{nextRandomValue}\n");
                _storage.Write(nextRandomValueBytes);
                _storageBytes = nextRandomValueBytes;
                RewriteStorageLines();
            }
        }
    }
}