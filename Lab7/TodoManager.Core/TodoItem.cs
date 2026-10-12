namespace TodoManager.Core;
//Приоритет задачи (чем больше, тем важнее)
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
//Задача менеджера. Любое изменение (например, выполнение)
//создаёт новую копию через <c>with</c>

/// <param name="Id">Уникальный номер задачи</param>
/// <param name="Title">Название</param>
/// <param name="Priority">Приоритет</param>
/// <param name="DueDate">Срок выполнения</param>
/// <param name="IsCompleted">Выполнена ли задача</param>
public record TodoItem(int Id, string Title, TaskPriority Priority, DateOnly DueDate, bool IsCompleted = false);