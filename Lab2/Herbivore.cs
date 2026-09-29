namespace Lab2;

//Травоядное: питается растительной пищей, 8% от массы тела в день
public class Herbivore : Animal
{
    private const double RationRatio = 0.08;

    public Herbivore(string name, double weight) : base(name, weight) { }

    public override string FoodType => "трава, сено";

    public override double DailyFoodKg() => Weight * RationRatio;
}