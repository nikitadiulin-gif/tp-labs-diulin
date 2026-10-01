using Lab3;

Console.Write("Введите путь к папке: ");
string? folder = Console.ReadLine();
try
{
    List<ExtensionStat> topExtensions = FolderStatsAnalyzer.GetTopExtensions(folder!, topCount: 5);

    if (topExtensions.Count == 0)
    {
        Console.WriteLine("В указанной папке нет файлов.");
        return;
    }

    Console.WriteLine("\nТоп-5 расширений по количеству файлов:");
    Console.WriteLine(new string('-', 50));

    foreach (ExtensionStat stat in topExtensions)
    {
        Console.WriteLine(
            $"{stat.Extension,-20} {stat.Count,5} шт.   {FolderStatsAnalyzer.FormatSize(stat.TotalSizeBytes),12}");
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Некорректный ввод: {ex.Message}");
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Недостаточно прав для доступа к папке или одной из вложенных папок.");
}
catch (IOException ex)
{
    Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
}