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
                    case "help":
                        Help();
                        break;
                    case "rand":
                        Random();
                        break;
                    default:
                        _userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                ClearStorageAndWriteCurrentRandom();
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

        private void InitializeStorageIfEmpty()
        {
            if (StorageBytes == null || StorageBytes.Length == 0)
            {
                const long firstRandomValue = 420L;
                AddToStorage(firstRandomValue);
            }
        }

        private void Add()
        {
            const int argumentsCountOfThisCommand = 2;

            var args = CurrentCommandLines?.Skip(1).ToList();

            var needToReadCount = argumentsCountOfThisCommand - args.Count;
            ReadFromConsoleNTimes(needToReadCount, args);

            var numbers = args.ConvertAll(int.Parse);
            _userConsole.WriteLine(numbers.Sum().ToString(Culture));
        }

        private void ReadFromConsoleNTimes(int needToReadCount, List<string> args)
        {
            for (var i = 0; i < needToReadCount; i++)
            {
                var value = ReadNumberFromConsole();
                args.Add(value.ToString());
                AddToStorage(value);
            }
        }

        private void Median()
        {
            var medianArgumentsFromStorage = CurrentCommandLines.Skip(1).ToList();

            int totalCountOfArguments;
            if (medianArgumentsFromStorage.Count != 0)
            {
                totalCountOfArguments = int.Parse(medianArgumentsFromStorage.First());
                medianArgumentsFromStorage = medianArgumentsFromStorage.Skip(1).ToList();
            }
            else
            {
                totalCountOfArguments = ReadNumberFromConsole();
                AddToStorage(totalCountOfArguments);
            }

            var remainingCountOfArguments = totalCountOfArguments - medianArgumentsFromStorage.Count;
            ReadFromConsoleNTimes(remainingCountOfArguments, medianArgumentsFromStorage);


            var numbers = medianArgumentsFromStorage.ConvertAll(int.Parse);
            var result = CalculateMedian(numbers);
            _userConsole.WriteLine(result.ToString(Culture));
        }

        private void Random()
        {
            const int a = 16807;
            const int m = 2147483647;

            var storageValues = CurrentCommandLines.Skip(1).ToList();

            int argumentsCountOfThisCommand;
            if (storageValues.Count != 0)
            {
                argumentsCountOfThisCommand = int.Parse(storageValues.First());
                storageValues = storageValues.Skip(1).ToList();
            }
            else
            {
                argumentsCountOfThisCommand = ReadNumberFromConsole();
                AddToStorage(argumentsCountOfThisCommand);
            }

            var restCountRandomValues = argumentsCountOfThisCommand - storageValues.Count;

            var x = NextRandomValue;

            for (var i = 0; i < restCountRandomValues; i++)
            {
                _userConsole.WriteLine(x.ToString(Culture));
                AddToStorage(x);
                x = a * x % m;
                RewriteCurrentRandomValue(x);
            }
        }

        private void RewriteCurrentRandomValue(long newRandomValue)
        {
            var valueBytes = UTF8.GetBytes($"{newRandomValue}\n");
            var rest = string.Join("\n", CurrentCommandLines) + "\n";
            _storage.Write(valueBytes.Concat(UTF8.GetBytes(rest)).ToArray());
        }

        private void ClearStorageAndWriteCurrentRandom()
        {
            var current = NextRandomValue;
            _storage.Write(Array.Empty<byte>());
            _storage.Write(UTF8.GetBytes($"{current}\n"));
        }

        private void AddToStorage(string value)
        {
            var valueBytes = UTF8.GetBytes($"{value}\n");
            _storage.Write(StorageBytes.Concat(valueBytes).ToArray());
        }

        private void AddToStorage(long value)
        {
            AddToStorage(value.ToString());
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