using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoManager.Core;
//Контракт хранилища задач. Менеджер зависит от этого интерфейса, а не от конкретного
//способа хранения: файл, память (для тестов) или что-то ещё подключается без правок менеджера
public interface ITaskStorage
{
    //Загружает все сохранённые задачи (пустой список, если сохранять ещё нечего)
    IReadOnlyList<TodoItem> Load();

    //Сохраняет полный список задач, заменяя предыдущее состояние
    void Save(IEnumerable<TodoItem> tasks);
}
//Хранение в памяти. Нужна для тестов и работы без файла
public class InMemoryTaskStorage : ITaskStorage
{
    private List<TodoItem> _items = [];
    public int SaveCount { get; private set; }
    public IReadOnlyList<TodoItem> Load() => _items.ToList();
    public void Save(IEnumerable<TodoItem> tasks)
    {
        _items = tasks.ToList(); // копия: хранилище не зависит от списка менеджера
        SaveCount++;
    }
}
//Хранение задач в JSON-файле
public class JsonFileTaskStorage : ITaskStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,  // для читаемости кириллицы
        Converters = { new JsonStringEnumConverter() }          // приоритет пишется словом: "High"
    };
    private readonly string _path;
    public JsonFileTaskStorage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));

        _path = path;
    }
    /// <exception cref="InvalidDataException">Файл существует, но содержит некорректные данные</exception>
    public IReadOnlyList<TodoItem> Load()
    {
        if (!File.Exists(_path))
            return [];

        string json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<TodoItem>>(json, Options) ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Файл '{_path}' повреждён: {ex.Message}", ex);
        }
    }
    public void Save(IEnumerable<TodoItem> tasks)
    {
        string json = JsonSerializer.Serialize(tasks, Options);
        File.WriteAllText(_path, json);
    }
}