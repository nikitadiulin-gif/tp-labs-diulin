using TodoManager.Core;
using Xunit;

namespace TodoManager.Tests;
public class TaskManagerTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);
    private static TaskManager NewManager() => new(new InMemoryTaskStorage());

    //Добавление
    [Fact]
    public void Add_ДвеЗадачи_ПрисваиваетПоследовательныеНомера()
    {
        var manager = NewManager();

        var first = manager.Add("Купить хлеб", TaskPriority.Low, Today);
        var second = manager.Add("Сдать лабу", TaskPriority.High, Today);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }
    [Fact]
    public void Add_ПустоеНазвание_БросаетИсключение()
    {
        var manager = NewManager();

        Assert.Throws<ArgumentException>(() => manager.Add("   ", TaskPriority.Low, Today));
    }

    //Выполнение
    [Fact]
    public void Complete_СуществующаяЗадача_ПомечаетВыполненной()
    {
        var manager = NewManager();
        var task = manager.Add("Сдать лабу", TaskPriority.High, Today);

        var result = manager.Complete(task.Id);

        Assert.True(result.IsCompleted);
        Assert.Single(manager.GetCompleted());
    }
    [Fact]
    public void Complete_НеизвестныйНомер_БросаетИсключение()
    {
        var manager = NewManager();

        Assert.Throws<KeyNotFoundException>(() => manager.Complete(42));
    }

    //Фильтры и сортировка
    [Fact]
    public void GetPending_ЕстьВыполненныеИНевыполненные_ВозвращаетТолькоНевыполненные()
    {
        var manager = NewManager();
        var done = manager.Add("Сделано", TaskPriority.Low, Today);
        var open = manager.Add("В работе", TaskPriority.Low, Today);
        manager.Complete(done.Id);

        var pending = manager.GetPending();

        Assert.Single(pending);
        Assert.Equal(open.Id, pending[0].Id);
    }
    [Fact]
    public void GetByPriority_РазныеПриоритеты_ВозвращаетТолькоСовпадающие()
    {
        var manager = NewManager();
        manager.Add("Низкая", TaskPriority.Low, Today);
        var high = manager.Add("Высокая", TaskPriority.High, Today);
        manager.Add("Средняя", TaskPriority.Medium, Today);

        var result = manager.GetByPriority(TaskPriority.High);

        Assert.Single(result);
        Assert.Equal(high.Id, result[0].Id);
    }
    [Fact]
    public void GetOverdue_РазныеСроки_ВозвращаетТолькоПросроченныеНевыполненные()
    {
        var manager = NewManager();
        var overdue = manager.Add("Просрочена", TaskPriority.High, Today.AddDays(-1));
        manager.Add("Срок сегодня", TaskPriority.High, Today);
        manager.Add("Срок завтра", TaskPriority.High, Today.AddDays(1));
        var doneLate = manager.Add("Выполнена с опозданием", TaskPriority.High, Today.AddDays(-5));
        manager.Complete(doneLate.Id);

        var result = manager.GetOverdue(Today);

        Assert.Single(result);
        Assert.Equal(overdue.Id, result[0].Id);
    }
    [Fact]
    public void GetAll_РазныеСроки_СортируетПоВозрастаниюСрока()
    {
        var manager = NewManager();
        var late = manager.Add("Позже", TaskPriority.Low, Today.AddDays(10));
        var soon = manager.Add("Скоро", TaskPriority.Low, Today.AddDays(1));

        var all = manager.GetAll();

        Assert.Equal(new[] { soon.Id, late.Id }, all.Select(t => t.Id));
    }

    //Хранилище
    [Fact]
    public void Add_СохраняетВХранилище_НовыйМенеджерВидитЗадачу()
    {
        var storage = new InMemoryTaskStorage();
        var first = new TaskManager(storage);
        first.Add("Сдать лабу", TaskPriority.High, Today);

        var second = new TaskManager(storage);

        Assert.Single(second.GetAll());
        Assert.Equal(1, storage.SaveCount);
    }
}
public class JsonFileTaskStorageTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"todo-test-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
    [Fact]
    public void SaveЗатемLoad_РусскиеНазванияИВсеПоля_СохраняютсяБезИзменений()
    {
        var storage = new JsonFileTaskStorage(_path);
        var tasks = new List<TodoItem>
        {
            new(1, "Сдать лабораторную", TaskPriority.High, new DateOnly(2026, 10, 20)),
            new(2, "Купить хлеб", TaskPriority.Low, new DateOnly(2026, 10, 13), IsCompleted: true)
        };

        storage.Save(tasks);
        var loaded = storage.Load().ToList();

        Assert.Equal(tasks, loaded);
    }
}