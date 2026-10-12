namespace TodoManager.Core;
//Менеджер задач: добавление, выполнение, фильтры. Хранилище передаётся через конструктор,
// поэтому менеджер не знает, где именно лежат данные
//Каждое изменение сразу сохраняется
public class TaskManager
{
    private readonly ITaskStorage _storage;
    private readonly List<TodoItem> _tasks;
    public TaskManager(ITaskStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _tasks = _storage.Load().ToList(); // загрузка при создании
    }
    //Добавляет задачу и сохраняет список
    /// <exception cref="ArgumentException">Пустое название</exception>
    /// <exception cref="ArgumentOutOfRangeException">Недопустимый приоритет</exception>
    public TodoItem Add(string title, TaskPriority priority, DateOnly dueDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (!Enum.IsDefined(priority))
            throw new ArgumentOutOfRangeException(nameof(priority), "Недопустимый приоритет.");

        int id = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
        var item = new TodoItem(id, title.Trim(), priority, dueDate);
        _tasks.Add(item);
        try
        {
            _storage.Save(_tasks);
        }
        catch
        {
            _tasks.Remove(item); // не сохранилось - в памяти тоже не оставляем
            throw;
        }

        return item;
    }
    //Отмечает задачу выполненной и сохраняет список
    /// <exception cref="KeyNotFoundException">Задачи с таким номером нет</exception>
    /// <exception cref="InvalidOperationException">Задача уже выполнена</exception>
    public TodoItem Complete(int id)
    {
        int index = _tasks.FindIndex(t => t.Id == id);
        if (index < 0)
            throw new KeyNotFoundException($"Задача с номером {id} не найдена.");

        TodoItem current = _tasks[index];
        if (current.IsCompleted)
            throw new InvalidOperationException($"Задача {id} уже выполнена.");

        TodoItem updated = current with { IsCompleted = true };
        _tasks[index] = updated;
        try
        {
            _storage.Save(_tasks);
        }
        catch
        {
            _tasks[index] = current;
            throw;
        }

        return updated;
    }
    public IReadOnlyList<TodoItem> GetAll() => Sorted(_tasks);
    public IReadOnlyList<TodoItem> GetPending() => Sorted(_tasks.Where(t => !t.IsCompleted));
    public IReadOnlyList<TodoItem> GetCompleted() => Sorted(_tasks.Where(t => t.IsCompleted));
    public IReadOnlyList<TodoItem> GetByPriority(TaskPriority priority) =>
        Sorted(_tasks.Where(t => t.Priority == priority));

    //Невыполненные задачи со сроком раньше <paramref name="today"/>
    public IReadOnlyList<TodoItem> GetOverdue(DateOnly today) =>
        Sorted(_tasks.Where(t => !t.IsCompleted && t.DueDate < today));

    //Порядок: ближайший срок, затем высокий приоритет, затем номер
    private static IReadOnlyList<TodoItem> Sorted(IEnumerable<TodoItem> items) =>
        items.OrderBy(t => t.DueDate)
             .ThenByDescending(t => t.Priority)
             .ThenBy(t => t.Id)
             .ToList();
}