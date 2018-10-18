using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AlgZad
{
    internal class Calculations
    {
        public static int N = 50;
        public static int argumentCount = 100000;
        public static int threadsCount = 10;

        public static int argumentsPerThread = (int)Math.Ceiling(Convert.ToDouble(argumentCount / threadsCount));

        public double Factorial(int x)
        {
            return x == 0 ? 1 : x * Factorial(x - 1);
        }

        public double AbsDiff(double x, double y)
        {
            return (x > y) ? x - y : y - x;
        }

        public double Power(double value, int power)
        {
            var originalValue = value;

            if (power == 0)
            {
                return 1;
            }

            if (power == 1 && value == 1)
            {
                return value;
            }

            if (value == -1)
            {
                return power % 2 == 0 ? 1 : -1;
            }

            for (int i = 0; i < power; i++)
            {
                value *= originalValue;
            }

            return value;
        }

        public double SinExp(double x, int n)
        {
            return Power(-1, n) * Power(x, 2 * n + 1) / Factorial(2 * n + 1);
        }

        public double AtgExp(double x, int n)
        {
            return Power(x, 2 * n + 1) * Power(-1, n) / (2 * n + 1);
        }

        public double Zad1(double x)
        {
            double sin = x;
            double atg = x;
            int n = 0;
            for (n = 1; n < N + 1; n++)
            {
                sin += SinExp(x, n);
            }

            for (n = 1; n < N + 1; n++)
            {
                atg += AtgExp(x, n);

            }

            return sin * atg;
        }

        public double Zad2(double x)
        {
            double sin = x;
            double atg = x;
            int n = 0;

            for (n = N; n >= 1; n--)
            {
                sin += SinExp(x, n);
            }

            for (n = N; n >= 1; n--)
            {
                atg += AtgExp(x, n);
            }

            return sin * atg;
        }

        public double Zad3(double x)
        {
            double sin = x;
            double atg = x;

            double currentSinExp = 0;
            double currentAtgExp = 0;

            var sinExpansions = new List<double>();
            var atgExpansions = new List<double>();

            sinExpansions.Add(x);
            atgExpansions.Add(x);

            for (int n = 1; n <= N; n++)
            {
                currentSinExp = SinExp(sinExpansions[n - 1], n);
                sinExpansions.Add(currentSinExp);
                sin += sinExpansions[n];

                currentAtgExp = AtgExp(atgExpansions[n - 1], n);
                atgExpansions.Add(currentAtgExp);
                atg += atgExpansions[n];
            }

            return sin * atg;
        }

        public double Zad4(double x)
        {
            double sin = x;
            double atg = x;

            double currentSinExp = 0;
            double currentAtgExp = 0;

            var sinExpansions = new List<double>();
            var atgExpansions = new List<double>();

            sinExpansions.Add(x);
            atgExpansions.Add(x);

            for (int n = N; n >= 1; n--)
            {
                currentSinExp = SinExp(sinExpansions[0], n);
                sinExpansions.Insert(0, currentSinExp);
                sin += sinExpansions[0];

                currentAtgExp = AtgExp(atgExpansions[0], n);
                atgExpansions.Insert(0, currentAtgExp);
                atg += atgExpansions[0];
            }

            return sin * atg;
        }

        public void RunCalculations(ConcurrentBag<List<double>> results, Random randomSeed, bool lastThread)
        {
            for (int i = 0; i < argumentsPerThread; i++)
            {
                var argument = (i % 2 == 0) ? randomSeed.NextDouble() : randomSeed.NextDouble() * -1;
                var question1Result = Zad1(argument);
                var question2Result = Zad2(argument);
                var question3Result = Zad3(argument);
                var question4Result = Zad4(argument);
                var buildInFunctionsResult = Math.Sin(argument) * Math.Atan(argument);

                results.Add(new List<double> {
                    argument,
                    buildInFunctionsResult,
                    question1Result,
                    AbsDiff(question1Result, buildInFunctionsResult),
                    question2Result,
                    AbsDiff(question2Result, buildInFunctionsResult),
                    question3Result,
                    AbsDiff(question3Result, buildInFunctionsResult),
                    question4Result,
                    AbsDiff(question4Result, buildInFunctionsResult),
                });

                if (lastThread)
                    Console.WriteLine(i*threadsCount);
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var randomSeed = new Random();
            var results = new ConcurrentBag<List<double>>();
            var threads = new List<Task>();
            var calculations = new Calculations();

            for (int i = 0; i < Calculations.threadsCount; i++)
            {
                var isLast = i == Calculations.threadsCount-1;
                threads.Add(new Task(() =>
                {
                    calculations.RunCalculations(results, randomSeed, isLast);
                }));
                threads[i].Start();
            }
            Task.WaitAll(threads.ToArray());

            CreateCsvFile(results);
            Console.WriteLine("Done");
            Console.ReadKey();

        }

        public static void CreateCsvFile(ConcurrentBag<List<double>> results)
        {
            var csvSeparator = ",";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filename = "zadAlg_" + Calculations.N + ".csv";

            var headers = new List<string>
            {
                "Argument",
                "Build in function result",
                "Question 1",
                "Error for question 1",
                "Question 2",
                "Error for question 2",
                "Question 3",
                "Error for question 3",
                "Question 4",
                "Error for question 4",
            };
            var csv = new StringBuilder();

            csv.Append(string.Join(csvSeparator, headers) + Environment.NewLine);

            foreach (var result in results)
            {
                csv.Append(string.Join(csvSeparator, result) + Environment.NewLine);
            }

            File.WriteAllText(desktopPath + "\\" + filename, csv.ToString());
        }
    }
}
