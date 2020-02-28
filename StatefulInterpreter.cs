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

        private UserConsole _userConsole;

        private Storage _storage;
        private byte[] StorageBytes => _storage.Read();

        private IEnumerable<string> StorageLines => UTF8.GetString(StorageBytes).Trim()
            .Split('\n').Where(line => !string.IsNullOrEmpty(line));

        private long NextRandomValue => System.Convert.ToInt64(StorageLines.First());
        private List<string> CurrentCommandLines => StorageLines.Skip(1).ToList();

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _storage = storage;
            _userConsole = userConsole;

            InitializeStorageIfEmpty();

            while (true)
            {
                var commandName = ReadCurrentCommandName(CurrentCommandLines);

                switch (commandName)
                {
                    case "exit":
                        return;
                    case "add":
                        Add();
                        break;
                    case "median":
                        Median();
                        break;
                    case "rand":
                        Random();
                        break;
                    case "help":
                        Help();
                        break;
                    default:
                        _userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                ClearStorageAndWriteNextRandom();
            }


            void InitializeStorageIfEmpty()
            {
                if (StorageBytes == null || StorageBytes.Length == 0)
                {
                    const long firstRandomValue = 420L;
                    AddToStorage(firstRandomValue);
                }
            }
        }

        private string ReadCurrentCommandName(List<string> storageLines)
        {
            string commandName;
            if (storageLines.Any())
            {
                commandName = storageLines.First();
            }
            else
            {
                commandName = _userConsole.ReadLine().Trim();
                AddToStorage(commandName);
            }

            return commandName;
        }

        private void Add()
        {
            const int totalCountOfArguments = 2;
            var argumentsFromStorage = CurrentCommandLines.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsFromStorage.Count;

            ReadFromConsoleAndAddToStorageFor(remainingCountOfArguments);

            var argumentsToExecuteAdd = CurrentCommandLines.Skip(1).ToList();
            var numbersToExecuteAdd = argumentsToExecuteAdd.ConvertAll(int.Parse);
            _userConsole.WriteLine(numbersToExecuteAdd.Sum().ToString(Culture));
        }

        private void ReadFromConsoleAndAddToStorageFor(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var value = ReadNumberFromConsole();
                AddToStorage(value);
            }
        }

        private void Median()
        {
            var argumentsFromStorage = CurrentCommandLines.Skip(1).ToList();
            var totalCountOfArguments = ReadTotalCountOfArguments(argumentsFromStorage);
            argumentsFromStorage = argumentsFromStorage.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsFromStorage.Count;

            ReadFromConsoleAndAddToStorageFor(remainingCountOfArguments);

            var argumentsToExecuteMedian = CurrentCommandLines.Skip(2).ToList();
            var numbersToExecuteMedian = argumentsToExecuteMedian.ConvertAll(int.Parse);
            _userConsole.WriteLine(CalculateMedian(numbersToExecuteMedian).ToString(Culture));
        }

        private int ReadTotalCountOfArguments(List<string> argumentsAtStart)
        {
            int totalCountOfArguments;

            if (argumentsAtStart.Count == 0)
            {
                totalCountOfArguments = ReadNumberFromConsole();
                AddToStorage(totalCountOfArguments);
            }
            else
            {
                totalCountOfArguments = int.Parse(argumentsAtStart.First());
            }

            return totalCountOfArguments;
        }

        private void Random()
        {
            const int a = 16807;
            const int m = 2147483647;

            var argumentsFromStorage = CurrentCommandLines.Skip(1).ToList();
            var totalCountOfArguments = ReadTotalCountOfArguments(argumentsFromStorage);
            argumentsFromStorage = argumentsFromStorage.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsFromStorage.Count;

            for (var i = 0; i < remainingCountOfArguments; i++)
            {
                var currentRandomValue = NextRandomValue;
                _userConsole.WriteLine(currentRandomValue.ToString(Culture));
                AddToStorage(currentRandomValue);
                
                ChangeCurrentRandomValueFor(a * currentRandomValue % m);
            }
        }

        private void ChangeCurrentRandomValueFor(long nextRandomValue)
        {
            var randomValueBytes = UTF8.GetBytes($"{nextRandomValue}\n");
            var currentCommandBytes = UTF8.GetBytes(string.Join("\n", CurrentCommandLines) + "\n");
            _storage.Write(randomValueBytes.Concat(currentCommandBytes).ToArray());
        }

        private void ClearStorageAndWriteNextRandom()
        {
            var nextRandomValue = NextRandomValue;
            _storage.Write(Array.Empty<byte>());
            _storage.Write(UTF8.GetBytes($"{nextRandomValue}\n"));
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

        private void Help()
        {
            const string exitMessage = "Чтобы выйти из режима помощи введите end";
            const string commands = "Доступные команды: add, median, rand";

            var helpLinesFromStorage = CurrentCommandLines.Skip(1).ToList();

            WriteMessageIfWasNotWritten("Укажите команду, для которой хотите посмотреть помощь");
            WriteMessageIfWasNotWritten(commands);
            WriteMessageIfWasNotWritten(exitMessage);

            while (true)
            {
                var commandName = ReadCurrentCommandName(helpLinesFromStorage);
                helpLinesFromStorage = helpLinesFromStorage.Skip(1).ToList();

                switch (commandName)
                {
                    case "end":
                        return;
                    case "add":
                        WriteMessageIfWasNotWritten("Вычисляет сумму двух чисел");
                        WriteMessageIfWasNotWritten(exitMessage);
                        break;
                    case "median":
                        WriteMessageIfWasNotWritten("Вычисляет медиану списка чисел");
                        WriteMessageIfWasNotWritten(exitMessage);
                        break;
                    case "rand":
                        WriteMessageIfWasNotWritten("Генерирует список случайных чисел");
                        WriteMessageIfWasNotWritten(exitMessage);
                        break;
                    default:
                        WriteMessageIfWasNotWritten("Такой команды нет");
                        WriteMessageIfWasNotWritten(commands);
                        WriteMessageIfWasNotWritten(exitMessage);
                        break;
                }
            }

            void WriteMessageIfWasNotWritten(string message)
            {
                if (helpLinesFromStorage.Count != 0)
                {
                    helpLinesFromStorage = helpLinesFromStorage.Skip(1).ToList();
                }
                else
                {
                    _userConsole.WriteLine(message);
                    AddToStorage(message);
                }
            }
        }

        private int ReadNumberFromConsole()
        {
            return int.Parse(_userConsole.ReadLine().Trim(), Culture);
        }
    }
}