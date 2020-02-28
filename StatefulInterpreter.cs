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
            .Split('\n').Where(com => !string.IsNullOrEmpty(com));

        private long NextRandomValue => System.Convert.ToInt64(StorageLines.First());
        private IEnumerable<string> CurrentCommandLines => StorageLines.Skip(1);

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _storage = storage;
            _userConsole = userConsole;

            InitializeStorageIfEmpty();

            while (true)
            {
                var commandName = GetCurrentCommandName();

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
        }

        private void InitializeStorageIfEmpty()
        {
            if (StorageBytes == null || StorageBytes.Length == 0)
            {
                const long firstRandomValue = 420L;
                AddToStorage(firstRandomValue);
            }
        }

        private string GetCurrentCommandName()
        {
            string commandName;
            if (CurrentCommandLines.Any())
            {
                commandName = CurrentCommandLines.First();
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
            var argumentsAtStart = CurrentCommandLines.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsAtStart.Count;

            ReadFromConsoleAndAddToStorageFor(remainingCountOfArguments);

            var argumentsAtEnd = CurrentCommandLines.Skip(1).ToList();
            var numbers = argumentsAtEnd.ConvertAll(int.Parse);
            _userConsole.WriteLine(numbers.Sum().ToString(Culture));
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
            var argumentsAtStart = CurrentCommandLines.Skip(1).ToList();
            var totalCountOfArguments = ReadTotalCountOfArguments(argumentsAtStart);
            argumentsAtStart = argumentsAtStart.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsAtStart.Count;

            ReadFromConsoleAndAddToStorageFor(remainingCountOfArguments);

            var argumentsExceptCountAtEnd = CurrentCommandLines.Skip(2).ToList();
            var numbers = argumentsExceptCountAtEnd.ConvertAll(int.Parse);
            _userConsole.WriteLine(CalculateMedian(numbers).ToString(Culture));
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

            var argumentsAtStart = CurrentCommandLines.Skip(1).ToList();
            var totalCountOfArguments = ReadTotalCountOfArguments(argumentsAtStart);
            argumentsAtStart = argumentsAtStart.Skip(1).ToList();
            var remainingCountOfArguments = totalCountOfArguments - argumentsAtStart.Count;

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

            var storageValues = CurrentCommandLines.Skip(1).ToList();

            string message;

            if (storageValues.Count != 0)
            {
                storageValues = storageValues.Skip(1).ToList();
            }
            else
            {
                message = "Укажите команду, для которой хотите посмотреть помощь";
                _userConsole.WriteLine(message);
                AddToStorage(message);
            }

            if (storageValues.Count != 0)
            {
                storageValues = storageValues.Skip(1).ToList();
            }
            else
            {
                message = commands;
                _userConsole.WriteLine(message);
                AddToStorage(message);
            }

            if (storageValues.Count != 0)
            {
                storageValues = storageValues.Skip(1).ToList();
            }
            else
            {
                message = exitMessage;
                _userConsole.WriteLine(message);
                AddToStorage(message);
            }

            while (true)
            {
                string command;

                if (storageValues.Count != 0)
                {
                    command = storageValues.First();
                    storageValues = storageValues.Skip(1).ToList();
                }
                else
                {
                    command = _userConsole.ReadLine().Trim();
                    AddToStorage(command);
                }

                switch (command)
                {
                    case "end":
                        return;
                    case "add":

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = "Вычисляет сумму двух чисел";
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = exitMessage;
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        break;
                    case "median":
                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = "Вычисляет медиану списка чисел";
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = exitMessage;
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        break;
                    case "rand":
                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = "Генерирует список случайных чисел";
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = exitMessage;
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        break;
                    default:
                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = "Такой команды нет";
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = commands;
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        if (storageValues.Count != 0)
                        {
                            storageValues = storageValues.Skip(1).ToList();
                        }
                        else
                        {
                            message = exitMessage;
                            _userConsole.WriteLine(message);
                            AddToStorage(message);
                        }

                        break;
                }
            }
        }

        private int ReadNumberFromConsole()
        {
            return int.Parse(_userConsole.ReadLine().Trim(), Culture);
        }
    }
}