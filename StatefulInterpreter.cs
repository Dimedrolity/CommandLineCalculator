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

        private ConsoleWithStorage _consoleWithStorage;

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _consoleWithStorage = new ConsoleWithStorage(userConsole, storage);

            var x = _consoleWithStorage.GetNextRandom();

            while (true)
            {
                var commandName = _consoleWithStorage.ReadLine();

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
                        x = Random(x);
                        _consoleWithStorage.ChangeCurrentRandomValueTo(x);
                        break;
                    // case "help":
                    //     Help();
                    //     break;
                    default:
                        _consoleWithStorage.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }

                _consoleWithStorage.CurrentCommandIsDone();
            }
        }


        private void Add()
        {
            var a = ReadNumberFromConsole();
            var b = ReadNumberFromConsole();
            _consoleWithStorage.WriteLine((a + b).ToString(Culture));
        }

        private int ReadNumberFromConsole()
        {
            return int.Parse(_consoleWithStorage.ReadLine().Trim(), Culture);
        }

        private void Median()
        {
            var count = ReadNumberFromConsole();
            var numbers = new List<int>();
            for (var i = 0; i < count; i++)
            {
                numbers.Add(ReadNumberFromConsole());
            }

            var result = CalculateMedian(numbers);
            _consoleWithStorage.WriteLine(result.ToString(Culture));
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


        private long Random(long x)
        {
            const int a = 16807;
            const int m = 2147483647;


            var count = ReadNumberFromConsole();
            for (var i = 0; i < count; i++)
            {
                _consoleWithStorage.WriteLine(x.ToString(Culture));
                x = a * x % m;
            }

            return x;
        }


        #region Help

        // private void Help()
        // {
        //     const string exitMessage = "Чтобы выйти из режима помощи введите end";
        //     const string commands = "Доступные команды: add, median, rand";
        //
        //     var helpLinesFromStorage = CurrentCommandLines.Skip(1).ToList();
        //
        //     WriteMessageIfWasNotWritten("Укажите команду, для которой хотите посмотреть помощь");
        //     WriteMessageIfWasNotWritten(commands);
        //     WriteMessageIfWasNotWritten(exitMessage);
        //
        //     while (true)
        //     {
        //         var commandName = ReadCurrentCommandName(helpLinesFromStorage);
        //         helpLinesFromStorage = helpLinesFromStorage.Skip(1).ToList();
        //
        //         switch (commandName)
        //         {
        //             case "end":
        //                 return;
        //             case "add":
        //                 WriteMessageIfWasNotWritten("Вычисляет сумму двух чисел");
        //                 WriteMessageIfWasNotWritten(exitMessage);
        //                 break;
        //             case "median":
        //                 WriteMessageIfWasNotWritten("Вычисляет медиану списка чисел");
        //                 WriteMessageIfWasNotWritten(exitMessage);
        //                 break;
        //             case "rand":
        //                 WriteMessageIfWasNotWritten("Генерирует список случайных чисел");
        //                 WriteMessageIfWasNotWritten(exitMessage);
        //                 break;
        //             default:
        //                 WriteMessageIfWasNotWritten("Такой команды нет");
        //                 WriteMessageIfWasNotWritten(commands);
        //                 WriteMessageIfWasNotWritten(exitMessage);
        //                 break;
        //         }
        //     }
        //
        //     void WriteMessageIfWasNotWritten(string message)
        //     {
        //         if (helpLinesFromStorage.Count != 0)
        //         {
        //             helpLinesFromStorage = helpLinesFromStorage.Skip(1).ToList();
        //         }
        //         else
        //         {
        //             _userConsole.WriteLine(message);
        //             AddToStorage(message);
        //         }
        //     }
        // }

        #endregion


        private class ConsoleWithStorage : UserConsole
        {
            private static CultureInfo Culture => CultureInfo.InvariantCulture;

            private UserConsole _console;
            private Storage _storage;

            private int _currentLine = 1;
            private byte[] StorageBytes => _storage.Read();

            private List<string> StorageLines => UTF8.GetString(StorageBytes).Trim()
                .Split('\n').Where(line => !string.IsNullOrEmpty(line)).ToList();

            // private long NextRandomValue => System.Convert.ToInt64(StorageLines.First());
            // private List<string> CurrentCommandLines => StorageLines.Skip(1).ToList();

            public ConsoleWithStorage(UserConsole console, Storage storage)
            {
                _console = console;
                _storage = storage;

                // _currentLine = StorageLines.Count();
                InitializeStorageIfEmpty();
            }

            private void InitializeStorageIfEmpty()
            {
                if (StorageBytes == null || StorageBytes.Length == 0)
                {
                    const long firstRandomValue = 420L;
                    AddToStorage(firstRandomValue);
                }

                // else
                // {
                //     _currentLine = StorageLines.Count();
                // }
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
                if (_currentLine < StorageLines.Count())
                {
                    readLine = StorageLines[_currentLine];
                }
                else
                {
                    readLine = _console.ReadLine().Trim();
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

            // public string ReadCurrentCommandName(List<string> storageLines)
            // {
            //     string commandName;
            //     if (storageLines.Any())
            //     {
            //         commandName = storageLines.First();
            //     }
            //     else
            //     {
            //         commandName = _console.ReadLine().Trim();
            //         AddToStorage(commandName);
            //     }
            //
            //     return commandName;
            // }

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