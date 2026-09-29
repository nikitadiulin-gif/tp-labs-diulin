using Lab2;

List<Animal> animals = new()
{
    new Predator("Лев Симба", 190),
    new Predator("Тигр Шерхан", 220),
    new Herbivore("Слон Дамбо", 4500),
    new Herbivore("Зебра Марти", 350),
    new Bird("Орёл Арчи", 5.5, 2.1),
    new Bird("Попугай Кеша", 0.4, 0.5)
};

Console.WriteLine("=== Зоопарк ===");

foreach (Animal a in animals)
    Console.WriteLine(a);

double totalFood = animals.Sum(a => a.DailyFoodKg());
Console.WriteLine($"\nСуммарный рацион зоопарка: {totalFood:F2} кг/день");

Console.WriteLine("\nПроверка защиты состояния");
try
{
    var bad = new Predator("Ошибка", -5);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Исключение: {ex.ParamName}");
}