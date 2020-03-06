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

            var nextRandomValue = consoleWithStorage.GetNextRandom();

            while (true)
            {
                var input = consoleWithStorage.ReadLine();
                switch (input.Trim())
                {
                    case "exit":
                        consoleWithStorage.ClearStorage();
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
                        nextRandomValue = Random(consoleWithStorage, nextRandomValue);
                        consoleWithStorage.ChangeCurrentRandomValueTo(nextRandomValue);
                        break;
                    default:
                        consoleWithStorage.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                consoleWithStorage.PrepareForNextCommand();
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

            private byte[] _storageBytes;
            private string[] _storageLines;
            private int _currentLine = 1;

            private const char StorageLinesSeparator = '\n';

            public ConsoleWithStorage(UserConsole console, Storage storage)
            {
                _console = console;
                _storage = storage;

                var storageContent = _storage.Read();
                RewriteStorageBuffersFor(storageContent);

                if (_storageBytes == null || _storageBytes.Length == 0)
                {
                    const long firstRandomValue = 420L;
                    AddToStorage(firstRandomValue);
                }
            }

            private void RewriteStorageBuffersFor(byte[] newBytes)
            {
                _storageBytes = newBytes;
                _storageLines = UTF8.GetString(_storageBytes).Trim().Split(StorageLinesSeparator)
                    .Where(line => !string.IsNullOrEmpty(line)).ToArray();
            }

            private void AddToStorage(long value)
            {
                AddToStorage(value.ToString(Culture));
            }

            private void AddToStorage(string value)
            {
                var valueBytes = UTF8.GetBytes($"{value}{StorageLinesSeparator}");

                var bytesWithNewValue = _storageBytes.Concat(valueBytes).ToArray();

                _storage.Write(bytesWithNewValue);
                RewriteStorageBuffersFor(bytesWithNewValue);
            }

            public override string ReadLine()
            {
                string readLine;
                if (_currentLine < _storageLines.Length)
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

            public override void WriteLine(string content)
            {
                var lastIndexOfStorageLines = _storageLines.Length - 1;
                var isCurrentLineOutsideOfStorageLines = _currentLine > lastIndexOfStorageLines;

                if (isCurrentLineOutsideOfStorageLines)
                {
                    _console.WriteLine(content);
                    AddToStorage(content);
                }

                _currentLine++;
            }

            public long GetNextRandom()
            {
                return System.Convert.ToInt64(_storageLines[0]);
            }

            public void PrepareForNextCommand()
            {
                _currentLine = 1;
                ClearStorageAndWriteNextRandom();

                void ClearStorageAndWriteNextRandom()
                {
                    var nextRandomValue = GetNextRandom();

                    ClearStorage();

                    var nextRandomValueBytes = UTF8.GetBytes($"{nextRandomValue}{StorageLinesSeparator}");

                    _storage.Write(nextRandomValueBytes);
                    RewriteStorageBuffersFor(nextRandomValueBytes);
                }
            }

            public void ClearStorage()
            {
                _storage.Write(Array.Empty<byte>());
            }
            
            public void ChangeCurrentRandomValueTo(long nextRandomValue)
            {
                var nextRandomValueBytes = UTF8.GetBytes($"{nextRandomValue}{StorageLinesSeparator}");

                var storageBytesExceptCurrentRandom = GetStorageBytesExceptCurrentRandom();

                var bytesWithNextRandomValue = nextRandomValueBytes.Concat(storageBytesExceptCurrentRandom).ToArray();

                _storage.Write(bytesWithNextRandomValue);
                RewriteStorageBuffersFor(bytesWithNextRandomValue);

                
                byte[] GetStorageBytesExceptCurrentRandom()
                {
                    var separatorAsSting = StorageLinesSeparator.ToString(Culture);

                    var storageLinesExceptCurrentRandom = _storageLines.Skip(1);
                    
                    var joinedStorageLinesExceptCurrentRandom =
                        string.Join(separatorAsSting, storageLinesExceptCurrentRandom)
                        + StorageLinesSeparator;
                    
                    return UTF8.GetBytes(joinedStorageLinesExceptCurrentRandom);
                }
            }
        }
    }
}