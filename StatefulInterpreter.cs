﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Text.Encoding;
using Convert = System.Convert;

namespace CommandLineCalculator
{
    public sealed class StatefulInterpreter : Interpreter
    {
        private static CultureInfo Culture => CultureInfo.InvariantCulture;

        private const long FirstRandomValue = 420L;

        private Storage _storage;
        private byte[] StorageBytes => _storage.Read().ToArray();

        private string[] StorageParts => UTF8.GetString(StorageBytes).Trim()
            .Split('\n').Where(com => !string.IsNullOrEmpty(com)).ToArray();

        private string[] StoragePartsWoRand => UTF8.GetString(StorageBytes).Trim()
            .Split('\n').Where(com => !string.IsNullOrEmpty(com)).Skip(1).ToArray();


        private long CurrentRandomValue => Convert.ToInt64(StorageParts.FirstOrDefault());

        public override void Run(UserConsole userConsole, Storage storage)
        {
            _storage = storage;
            if (CurrentRandomValue == 0)
            {
                var randomValueBytes = UTF8.GetBytes(FirstRandomValue.ToString());
                _storage.Write(randomValueBytes.Concat(UTF8.GetBytes("\n")).ToArray());
            }

            var x = CurrentRandomValue;

            while (true)
            {
                var commandName = StoragePartsWoRand == null || StoragePartsWoRand.Length == 0
                    ? userConsole.ReadLine().Trim()
                    : StoragePartsWoRand[0];

                switch (commandName)
                {
                    case "exit":
                        return;
                    case "add":
                        Add_New(userConsole);
                        break;
                    case "median":
                        Median_New(userConsole);
                        break;
                    case "help":
                        Help(userConsole);
                        break;
                    case "rand":
                        x = Random_New(userConsole, x);
                        break;
                    default:
                        userConsole.WriteLine("Такой команды нет, используйте help для списка команд");
                        break;
                }
            }
        }

        private void Add_New(UserConsole console)
        {
            if (StoragePartsWoRand == null || StoragePartsWoRand.Length == 0)
                AddToStorage("add");

            const int argumentsCountOfThisCommand = 2;

            var args = StoragePartsWoRand?.Skip(1).ToList();

            var needToReadCount = argumentsCountOfThisCommand - args.Count;
            ReadFromConsoleNTimes(console, needToReadCount, args);

            var numbers = args.ConvertAll(int.Parse);
            console.WriteLine(numbers.Sum().ToString(Culture)); //result

            ClearStorageAndWriteCurrentRandom();
        }

        private void ReadFromConsoleNTimes(UserConsole console, int needToReadCount, List<string> args)
        {
            for (var i = 0; i < needToReadCount; i++)
            {
                var value = ReadNumber(console);
                args.Add(value.ToString());
                AddToStorage(value);
            }
        }

        private void Median_New(UserConsole console)
        {
            if (StoragePartsWoRand == null || StoragePartsWoRand.Length == 0)
            {
                AddToStorage("median");
            }

            var args = StoragePartsWoRand.Skip(1).ToList();

            int argumentsCountOfThisCommand;
            if (args.Count != 0)
            {
                argumentsCountOfThisCommand = int.Parse(args.First());
                args = args.Skip(1).ToList();
            }
            else
            {
                argumentsCountOfThisCommand = ReadNumber(console);
                AddToStorage(argumentsCountOfThisCommand);
            }

            var needToReadCount = argumentsCountOfThisCommand - args.Count;
            ReadFromConsoleNTimes(console, needToReadCount, args);


            var numbers = args.ConvertAll(int.Parse);
            var result = CalculateMedian(numbers);
            console.WriteLine(result.ToString(Culture));

            ClearStorageAndWriteCurrentRandom();
        }

        private long Random_New(UserConsole console, long x)
        {
            const int a = 16807;
            const int m = 2147483647;

            if (StoragePartsWoRand == null || StoragePartsWoRand.Length == 0)
            {
                AddToStorage("rand");
            }

            var storageValues = StoragePartsWoRand.Skip(1).ToList();

            int argumentsCountOfThisCommand;
            if (storageValues.Count != 0)
            {
                argumentsCountOfThisCommand = int.Parse(storageValues.First());
                storageValues = storageValues.Skip(1).ToList();
            }
            else
            {
                argumentsCountOfThisCommand = ReadNumber(console);
                AddToStorage(argumentsCountOfThisCommand);
            }

            var restCountRandomValues = argumentsCountOfThisCommand - storageValues.Count;

            for (var i = 0; i < restCountRandomValues; i++)
            {
                console.WriteLine(x.ToString(Culture));
                AddToStorage(x);
                x = a * x % m;
                RewriteCurrentRandomValue(x);
            }

            ClearStorageAndWriteCurrentRandom();
            return CurrentRandomValue;
        }

        private void RewriteCurrentRandomValue(long newRandomValue)
        {
            var valueBytes = UTF8.GetBytes($"{newRandomValue}\n");
            var rest = string.Join("\n", StoragePartsWoRand);
            _storage.Write(valueBytes.Concat(UTF8.GetBytes(rest)).ToArray());
        }

        private void ClearStorageAndWriteCurrentRandom()
        {
            var current = CurrentRandomValue;
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
            var valueBytes = UTF8.GetBytes($"{value}\n");
            _storage.Write(StorageBytes.Concat(valueBytes).ToArray());
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