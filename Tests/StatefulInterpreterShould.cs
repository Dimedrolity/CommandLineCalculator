﻿using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using static CommandLineCalculator.Tests.TestConsole.Action;

namespace CommandLineCalculator.Tests
{
    public class StatefulInterpreterShould
    {
        public static TestCaseData[] RegularCases =>
            new[]
            {
                new TestCaseData(
                    new TestConsole(
                        (Read, "exit")
                    )
                ).SetName("exit"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "add"),
                        (Read, "15"),
                        (Read, "60"),
                        (Write, "75"),
                        (Read, "exit")
                    )
                ).SetName("add"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "median"),
                        (Read, "5"),
                        (Read, "17"),
                        (Read, "30"),
                        (Read, "29"),
                        (Read, "23"),
                        (Read, "20"),
                        (Write, "23"),
                        (Read, "exit")
                    )
                ).SetName("odd median"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "median"),
                        (Read, "6"),
                        (Read, "17"),
                        (Read, "30"),
                        (Read, "29"),
                        (Read, "23"),
                        (Read, "20"),
                        (Read, "24"),
                        (Write, "23.5"),
                        (Read, "exit")
                    )
                ).SetName("even median"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "rand"),
                        (Read, "1"),
                        (Write, "420"),
                        (Read, "exit")
                    )
                ).SetName("rand 1"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "rand"),
                        (Read, "1"),
                        (Write, "420"),
                        (Read, "rand"),
                        (Read, "2"),
                        (Write, "7058940"),
                        (Write, "528003995"),
                        (Read, "rand"),
                        (Read, "3"),
                        (Write, "760714561"),
                        (Write, "1359476136"),
                        (Write, "1636897319"),
                        (Read, "exit")
                    )
                ).SetName("rand 3x"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "ramd"),
                        (Write, "Такой команды нет, используйте help для списка команд"),
                        (Read, "exit")
                    )
                ).SetName("unknown command"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("empty help"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "add"),
                        (Write, "Вычисляет сумму двух чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("add help"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "median"),
                        (Write, "Вычисляет медиану списка чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("median help"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "rand"),
                        (Write, "Генерирует список случайных чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("rand help"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "media"),
                        (Write, "Такой команды нет"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("unknown help"),
                new TestCaseData(
                    new TestConsole(
                        (Read, "help"),
                        (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "media"),
                        (Write, "Такой команды нет"),
                        (Write, "Доступные команды: add, median, rand"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "add"),
                        (Write, "Вычисляет сумму двух чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "rand"),
                        (Write, "Генерирует список случайных чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "median"),
                        (Write, "Вычисляет медиану списка чисел"),
                        (Write, "Чтобы выйти из режима помощи введите end"),
                        (Read, "end"),
                        (Read, "exit")
                    )
                ).SetName("several commands help"),
            };

        [Test]
        [TestCaseSource(nameof(RegularCases))]
        public void Run_As_Expected(TestConsole console)
        {
            var storage = new MemoryStorage();
            var interpreter = new StatefulInterpreter();
            interpreter.Run(console, storage);
            console.AtEnd.Should().BeTrue();
        }

        public static TestCaseData[] InterruptionCases => new[]
        {
            new TestCaseData(
                new TestConsole(
                    (Read, "add"),
                    (Read, "15"),
                    (Read, "60"),
                    (Write, "75"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8}
            ).SetName("add"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "3"),
                    (Read, "60"),
                    (Read, "50"),
                    (Read, "41"),
                    (Write, "50"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10}
            ).SetName("median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "420"),
                    (Write, "7058940"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8}
            ).SetName("rand"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "1"),
                    (Write, "420"),
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Read, "rand"),
                    (Read, "3"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24}
            ).SetName("my rand x3"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "3"),
                    (Write, "420"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Read, "median"),
                    (Read, "3"),
                    (Read, "60"),
                    (Read, "50"),
                    (Read, "41"),
                    (Write, "50"),
                    (Read, "rand"),
                    (Read, "3"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32}
            ).SetName("rand median rand"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "420"),
                    (Write, "7058940"),
                    (Read, "add"),
                    (Read, "2"),
                    (Read, "3"),
                    (Write, "5"),
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "528003995"),
                    (Write, "760714561"),
                    (Read, "rand"),
                    (Read, "2"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "add"),
                    (Read, "2"),
                    (Read, "3"),
                    (Write, "5"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34}
            ).SetName("rand 3x help add"),
            new TestCaseData(
                new TestConsole(
                    (Read, "rand"),
                    (Read, "6"),
                    (Write, "420"),
                    (Write, "7058940"),
                    (Write, "528003995"),
                    (Write, "760714561"),
                    (Write, "1359476136"),
                    (Write, "1636897319"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16}
            ).SetName("many rand"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4}
            ).SetName("help end"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "add"),
                    (Write, "Вычисляет сумму двух чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16}
            ).SetName("help add break"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "median"),
                    (Write, "Вычисляет медиану списка чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16}
            ).SetName("help median break"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "azazaza"),
                    (Write, "Такой команды нет"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18,}
            ).SetName("help unknown command"),
            new TestCaseData(
                new TestConsole(
                    (Read, "help"),
                    (Write, "Укажите команду, для которой хотите посмотреть помощь"),
                    (Write, "Доступные команды: add, median, rand"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "rand"),
                    (Write, "Генерирует список случайных чисел"),
                    (Write, "Чтобы выйти из режима помощи введите end"),
                    (Read, "end"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6, 8, 10, 12, 14, 16}
            ).SetName("help rand break"),
            new TestCaseData(
                new TestConsole(
                    (Read, "median"),
                    (Read, "0"),
                    (Write, "0"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4, 6}
            ).SetName("zero median"),
            new TestCaseData(
                new TestConsole(
                    (Read, "ramd"),
                    (Write, "Такой команды нет, используйте help для списка команд"),
                    (Read, "exit")
                ),
                new[] {0, 2, 4,}
            ).SetName("unknown command")
        };


        [Test]
        [TestCaseSource(nameof(InterruptionCases))]
        public void Run_With_Interruptions(
            TestConsole console,
            int[] failureSchedule)
        {
            var storage = new MemoryStorage();
            var brokenConsole = new BrokenConsole(console, failureSchedule);
            for (var i = 0; i < failureSchedule.Length; i++)
            {
                var exception = Assert.Throws<TestException>(() =>
                {
                    var interpreter = new StatefulInterpreter();
                    interpreter.Run(brokenConsole, storage);
                });
                exception.Type.Should().Be(TestException.ExceptionType.InducedFailure);
            }

            var finalInterpreter = new StatefulInterpreter();
            finalInterpreter.Run(brokenConsole, storage);

            console.AtEnd.Should().BeTrue();
        }

        [Test]
        public void Convert()
        {
            var storage = new FileStorage(@"D:\TESTCalcFileStorage.txt");
            var message = "add 2 5";
            storage.Read();
            storage.Write(Encoding.UTF8.GetBytes(message));
            var actual = Encoding.UTF8.GetString(storage.Read());
            Assert.AreEqual(message, actual);
        }
    }
}