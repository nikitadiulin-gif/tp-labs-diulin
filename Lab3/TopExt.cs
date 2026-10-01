namespace Lab3;

//статистика по одному расширению файлов: сколько файлов и каков их суммарный размер
//<param name="Extension"> Расширение в нижнем регистре </param>
//<param name="Count"> Количество файлов с этим расширением </param>
//<param name="TotalSizeBytes"> Суммарный размер всех файлов этого расширения в байтах </param>
public record ExtensionStat(string Extension, int Count, long TotalSizeBytes);

//анализ файлов в папке: подсчёт самых часто встречающихся расширений
//и суммарного размера файлов по каждому расширению

public static class FolderStatsAnalyzer
{
    private const string NoExtensionLabel = "(без расширения)";
    //возвращает топ-N расширений по количеству файлов в папке и её подпапках,
    //вместе с суммарным размером файлов каждого расширения
    //<param name="folderPath"> Путь к папке </param>
    //<param name="topCount"> Сколько расширений вернуть (по умолчанию 5) </param>
    //<exception cref="ArgumentException"> Пустой путь </exception>
    //<exception cref="DirectoryNotFoundException"> Папка не существует </exception>
    public static List<ExtensionStat> GetTopExtensions(string folderPath, int topCount = 5)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));
        if (topCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(topCount), "Количество расширений должно быть положительным.");
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Папка не найдена: {folderPath}");
        var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

        return files
            .Select(path => new FileInfo(path))
            .GroupBy(info => NormalizeExtension(info.Extension))
            .Select(group => new ExtensionStat(
                Extension: group.Key,
                Count: group.Count(),
                TotalSizeBytes: group.Sum(f => f.Length)))
            .OrderByDescending(stat => stat.Count)
            .Take(topCount)
            .ToList();
    }
    //переводит расширение в единый формат (нижний регистр, подпись для файлов без расширения)
    private static string NormalizeExtension(string extension) =>
        string.IsNullOrEmpty(extension) ? NoExtensionLabel : extension.ToLowerInvariant();
    //переводит размер в байтах в удобочитаемую строку (Б / КБ / МБ)
    public static string FormatSize(long bytes)
    {
        const double kb = 1024.0;
        const double mb = kb * 1024.0;
        return bytes switch
        {
            < 0 => throw new ArgumentOutOfRangeException(nameof(bytes), "Размер не может быть отрицательным."),
            var b when b < kb => $"{b} Б",
            var b when b < mb => $"{b / kb:F2} КБ",
            var b => $"{b / mb:F2} МБ"
        };
    }
}