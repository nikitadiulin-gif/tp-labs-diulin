namespace Lab4.Core;

//Хищник: питается мясом, около 3% от массы тела в день
public class Predator : Animal
{
    private const double RationRatio = 0.03;

    public Predator(string name, double weight) : base(name, weight) { }

    public override string FoodType => "мясо";

    public override double DailyFoodKg() => Weight * RationRatio;
}