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
                        Console.WriteLine("Ошибка: введите число от 1 до 5.\n");
                        break;
                }
                if (!exit)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }
        static void Menu()
        {
            Console.WriteLine("Выберите задание:");
            Console.WriteLine("  1. Факториал");
            Console.WriteLine("  2. Числа Фибоначчи");
            Console.WriteLine("  3. Вычисление функции");
            Console.WriteLine("  4. Ряд Тейлора (arctg x)");
            Console.WriteLine("  5. Выход");
            Console.Write("Ваш выбор: ");
        }
        //вычисление факториала
        static void Factorial()
        {
            Console.WriteLine("\nЗадание 1: вычисление факториала");
            Console.Write("Введите n (0..20): ");

            if (!int.TryParse(Console.ReadLine(), out int n) || n < 0 || n > 20)
            {
                Console.WriteLine("Ошибка: введите число от 0 до 20.");
                return;
            }

            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;

            Console.WriteLine($"{n}! = {result}");
        }
        //вычисление чисел Фибоначчи
        static void Fibonacci()
        {
            Console.WriteLine("\nЗадание 2: числа Фибоначчи");
            Console.Write("Введите n (0..40): ");

            if (!int.TryParse(Console.ReadLine(), out int n) || n < 0)
            {
                Console.WriteLine("Ошибка: введите число от 0 до 40.");
                return;
            }
            if (n == 0)
            {
                Console.WriteLine("Последовательность Фибоначчи (0..0): 0");
                return;
            }
            long a = 0, b = 1;
            string result = "0, 1";

            for (int i = 2; i <= n; i++)
            {
                long next = a + b;
                result += $", {next}";
                a = b;
                b = next;
            }
            Console.WriteLine($"Последовательность Фибоначчи (0..{n}): {result}");
        }
        static void Function()
        {
            Console.WriteLine("\nЗадание 3: вычисление функции");
            Console.Write("Введите x: ");
            if (!double.TryParse(Console.ReadLine(), out double x))
            {
                Console.WriteLine("Ошибка: введите корректное число.");
                return;
            }
            //ОДЗ: x > 0 (для логарифма) и ln(4/x) >= 0 (под корнем)
            if (x <= 0)
            {
                Console.WriteLine("Ошибка: x должен быть > 0");
                return;
            }
            //проверка: ln(4/x) >= 0, значит  4/x >= 1  =>  x <= 4
            if (x > 4)
            {
                Console.WriteLine($"Ошибка: под корнем отрицательное число (ln(4/{x:F4}) < 0)");
                return;
            }
            double ln = Math.Log(4.0 / x);          //логарифм
            double sqrt = Math.Sqrt(lnPart);        //корень
            double division = 1.0 / x;              //деление
            double exp = Math.Exp(Math.Sin(x));     //экспонента

            double result = sqrtPart - divisionPart - expPart;
            Console.WriteLine($"A({x:F4}) = {result:F6}");
        }
    }
}
