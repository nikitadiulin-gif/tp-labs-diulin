using System;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Лабораторная работа 1. Вариант 5\n");
            bool exit = false;

            while (!exit)
            {
                Menu();
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Factorial();
                        break;
                    case "2":
                        Fibonacci();
                        break;
                    case "3":
                        Function();
                        break;
                    case "4":
                        Taylor();
                        break;
                    case "5":
                        exit = true;
                        Console.WriteLine("Выход");
                        break;
                    default:
                        Console.WriteLine("Ошибка: введите число от 0 до 20.\n");
                        break;
                }
            }
        }
    }
}
//Lab1. Вычисление факториала
Console.Write("Введите n (0..20): ");

if (!int .TryParse(Console.ReadLine(), out int n) || n < 0 || n > 20)
{
    Console.WriteLine("Ошибка: нужно целое число от 0 до 20.");
    return;
}

Console.WriteLine($"{n}! = {Factorial(n)}");

static long Factorial (int n)
{
    long result = 1;
    for (int i = 2; i <= n; i++)
        result *= i;
    return result;
}