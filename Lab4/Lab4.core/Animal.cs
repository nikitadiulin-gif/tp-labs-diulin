namespace Lab4.Core;

//Абстрактный базовый класс животного зоопарка (из лабораторной работы 2, вариант 5)
public abstract class Animal
{
    private readonly string _name;
    private readonly double _weight;

    public string Name => _name;

    //Вес животного, кг
    public double Weight => _weight;

    protected Animal(string name, double weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым.", nameof(name));
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Вес должен быть положительным.");

        _name = name;
        _weight = weight;
    }

    //Вид корма
    public abstract string FoodType { get; }

    //Суточный рацион, кг корма в день (каждый наследник считает по-своему)
    public abstract double DailyFoodKg();

    public override string ToString() =>
        $"{GetType().Name} {Name}: {Weight:F1} кг, корм: {FoodType}, рацион: {DailyFoodKg():F2} кг/день";
}