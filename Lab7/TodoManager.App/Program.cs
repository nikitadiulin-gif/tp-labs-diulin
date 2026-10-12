using System.Globalization;
using ConsoleTables;
using TodoManager.Core;

//Выбирается конкретное хранилище и передаётся менеджеру.
//Файл задач можно указать первым аргументом, по умолчанию tasks.json
string path = args.Length > 0 ? args[0] : "tasks.json";

try
{
    ITaskStorage storage = new JsonFileTaskStorage(path);
    var manager = new TaskManager(storage); // задачи загружаются при создании менеджера

    Console.WriteLine($"Файл задач: {Path.GetFullPath(path)}");
    new ConsoleMenu(manager).Run();
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Не удалось загрузить задачи: {ex.Message}");
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.WriteLine($"Ошибка доступа к файлу задач: {ex.Message}");
}

//Консольный интерфейс менеджера задач. Весь ввод-вывод здесь, в библиотеке Console нет
internal class ConsoleMenu
{
    private const string DateFormat = "dd.MM.yyyy";
    private readonly TaskManager _manager;
    public ConsoleMenu(TaskManager manager)
    {
        _manager = manager;
    }
    public void Run()
    {
        while (true)
        {
            PrintMenu();
            string? choice = Console.ReadLine();
            if (choice is null)
                return;
            try
            {
                if (!HandleChoice(choice.Trim()))
                    return;
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or ArgumentException)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.WriteLine($"Не удалось сохранить задачи в файл: {ex.Message}");
            }
        }
    }
    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Менеджер задач");
        Console.WriteLine("1 - Добавить задачу");
        Console.WriteLine("2 - Все задачи");
        Console.WriteLine("3 - В работе");
        Console.WriteLine("4 - Выполненные");
        Console.WriteLine("5 - Фильтр по приоритету");
        Console.WriteLine("6 - Просроченные");
        Console.WriteLine("7 - Отметить задачу выполненной");
        Console.WriteLine("0 - Выход");
        Console.Write("Ваш выбор: ");
    }

    //Возвращает false, если пользователь выбрал выход
    private bool HandleChoice(string choice)
    {
        switch (choice)
        {
            case "1": AddTask(); break;
            case "2": ShowTasks("Все задачи", _manager.GetAll()); break;
            case "3": ShowTasks("В работе", _manager.GetPending()); break;
            case "4": ShowTasks("Выполненные", _manager.GetCompleted()); break;
            case "5": ShowByPriority(); break;
            case "6": ShowTasks("Просроченные", _manager.GetOverdue(Today())); break;
            case "7": CompleteTask(); break;
            case "0": return false;
            default: Console.WriteLine("Ошибка: выберите пункт от 0 до 7."); break;
        }

        return true;
    }
    private void AddTask()
    {
        Console.Write("Название: ");
        string? title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Ошибка: название не может быть пустым.");
            return;
        }
        TaskPriority? priority = ReadPriority();
        if (priority is null)
            return;
        DateOnly? dueDate = ReadDate();
        if (dueDate is null)
            return;
        TodoItem item = _manager.Add(title, priority.Value, dueDate.Value);
        Console.WriteLine($"Добавлена задача {item.Id}.");
    }
    private void ShowByPriority()
    {
        TaskPriority? priority = ReadPriority();
        if (priority is null)
            return;

        ShowTasks($"Приоритет: {PriorityName(priority.Value)}", _manager.GetByPriority(priority.Value));
    }
    private void CompleteTask()
    {
        Console.Write("Номер задачи: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Ошибка: введите число.");
            return;
        }

        _manager.Complete(id);
        Console.WriteLine($"Задача {id} отмечена выполненной.");
    }
    private static void ShowTasks(string title, IReadOnlyList<TodoItem> tasks)
    {
        Console.WriteLine($"\n{title}:");
        if (tasks.Count == 0)
        {
            Console.WriteLine("Задач нет.");
            return;
        }
        DateOnly today = Today();
        var table = new ConsoleTable("№", "Задача", "Приоритет", "Срок", "Статус");
        foreach (TodoItem task in tasks)
        {
            table.AddRow(
                task.Id,
                task.Title,
                PriorityName(task.Priority),
                task.DueDate.ToString(DateFormat, CultureInfo.InvariantCulture),
                StatusName(task, today));
        }
        table.Write();
    }
    private static TaskPriority? ReadPriority()
    {
        while (true)
        {
            Console.Write("Приоритет (1 - низкий, 2 - средний, 3 - высокий): ");
            string? input = Console.ReadLine();
            if (input is null)
                return null;
            switch (input.Trim())
            {
                case "1": return TaskPriority.Low;
                case "2": return TaskPriority.Medium;
                case "3": return TaskPriority.High;
                default: Console.WriteLine("Ошибка: введите 1, 2 или 3."); break;
            }
        }
    }
    private static DateOnly? ReadDate()
    {
        while (true)
        {
            Console.Write("Срок (дд.мм.гггг): ");
            string? input = Console.ReadLine();
            if (input is null)
                return null;

            if (DateOnly.TryParseExact(input.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
                return date;
            Console.WriteLine("Ошибка: введите дату в формате дд.мм.гггг, например 25.09.2026.");
        }
    }
    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
    private static string PriorityName(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "низкий",
        TaskPriority.Medium => "средний",
        TaskPriority.High => "высокий",
        _ => priority.ToString()
    };
    private static string StatusName(TodoItem task, DateOnly today)
    {
        if (task.IsCompleted)
            return "выполнена";
        return task.DueDate < today ? "ПРОСРОЧЕНА" : "в работе";
    }
}