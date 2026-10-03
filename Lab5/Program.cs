using System.Diagnostics;
using Lab5;

Console.WriteLine("Лабораторная работа 5. Вариант 5: Шифр Виженера\n");

//Демонстрация шифрования/дешифрования
const string demoText = "ATTACKATDAWN";
const string demoKey = "LEMON";

string encrypted = VigenereCipher.Encrypt(demoText, demoKey);
string decrypted = VigenereCipher.Decrypt(encrypted, demoKey);

Console.WriteLine($"Исходный текст: {demoText}");
Console.WriteLine($"Ключ:           {demoKey}");
Console.WriteLine($"Зашифровано:    {encrypted}");
Console.WriteLine($"Расшифровано:   {decrypted}\n");

//Взлом шифра перебором ключей
Console.Write("Введите длину ключа для взлома (например, 5): ");
if (!int.TryParse(Console.ReadLine(), out int keyLength) || keyLength <= 0)
{
    Console.WriteLine("Ошибка: длина ключа должна быть положительным числом.");
    return;
}
Console.Write("Введите известное слово, которое должно встретиться после расшифровки: ");
string? knownWord = Console.ReadLine();
if (string.IsNullOrWhiteSpace(knownWord))
{
    Console.WriteLine("Ошибка: известное слово не может быть пустым.");
    return;
}
Console.Write("Введите степень параллелизма (количество потоков, например 4): ");
if (!int.TryParse(Console.ReadLine(), out int degreeOfParallelism) || degreeOfParallelism <= 0)
{
    Console.WriteLine("Ошибка: степень параллелизма должна быть положительным числом.");
    return;
}
string cipherText = encrypted;
long totalCombinations = VigenereCracker.TotalCombinations(keyLength);
Console.WriteLine($"\nВсего вариантов ключей для перебора: {totalCombinations:N0}\n");
try
{
    Console.WriteLine("--- Последовательный перебор ---");
    var swSequential = Stopwatch.StartNew();
    CrackResult sequentialResult = VigenereCracker.CrackSequential(cipherText, keyLength, knownWord);
    swSequential.Stop();

    Console.WriteLine($"Найденный ключ:    {sequentialResult.Key ?? "не найден"}");
    Console.WriteLine($"Проверено вариантов: {sequentialResult.CombinationsChecked:N0}");
    Console.WriteLine($"Время:             {swSequential.ElapsedMilliseconds} мс\n");

    Console.WriteLine("--- Параллельный перебор ---");
    var swParallel = Stopwatch.StartNew();
    CrackResult parallelResult = VigenereCracker.CrackParallel(cipherText, keyLength, knownWord, degreeOfParallelism);
    swParallel.Stop();

    Console.WriteLine($"Найденный ключ:    {parallelResult.Key ?? "не найден"}");
    Console.WriteLine($"Проверено вариантов: {parallelResult.CombinationsChecked:N0}");
    Console.WriteLine($"Время:             {swParallel.ElapsedMilliseconds} мс\n");

    Console.WriteLine("--- Итоги ---");
    bool keysMatch = sequentialResult.Key == parallelResult.Key;
    Console.WriteLine($"Результаты совпадают: {(keysMatch ? "да" : "НЕТ — ошибка в реализации!")}");

    double speedup = swParallel.ElapsedMilliseconds > 0
        ? (double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds
        : 0;
    Console.WriteLine($"Ускорение: {speedup:F2}x (потоков: {degreeOfParallelism}, доступно ядер: {Environment.ProcessorCount})");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Некорректный ввод: {ex.Message}");
}