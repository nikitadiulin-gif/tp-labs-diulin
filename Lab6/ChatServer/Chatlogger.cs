namespace ChatServer;

//Журнал сервера: пишет события в консоль и дозаписывает их в файл с метками времени
public class ChatLogger
{
    private readonly string _path;
    private readonly object _lock = new();
    public ChatLogger(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Путь к файлу журнала не может быть пустым.", nameof(path));
        _path = path;
    }
    public void Log(string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(line);

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_path, line + Environment.NewLine);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // сбой записи в журнал не должен ронять сервер
                Console.WriteLine($"Не удалось записать в журнал: {ex.Message}");
            }
        }
    }
}