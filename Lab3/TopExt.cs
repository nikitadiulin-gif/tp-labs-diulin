namespace Lab3;

//статистика по одному расширению файлов: сколько файлов и каков их суммарный размер
//<param name="Extension"> Расширение в нижнем регистре </param>
//<param name="Count"> Количество файлов с этим расширением </param>
//<param name="TotalSizeBytes"> Суммарный размер всех файлов этого расширения в байтах </param>
public record ExtensionStat(string Extension, int Count, long TotalSizeBytes);
//анализ файлов в папке: подсчёт расширений и их суммарного размера
public static class FolderStatsAnalyzer
{
    public static List<ExtensionStat> GetTopExtensions(string folderPath, int topCount = 5)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));

        if (topCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(topCount), "Количество расширений должно быть положительным.");

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Папка не найдена: {folderPath}");

        throw new NotImplementedException();
    }
}