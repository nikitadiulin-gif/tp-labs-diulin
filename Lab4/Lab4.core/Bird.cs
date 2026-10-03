namespace Lab4.Core;

//Птица: питается зерном и насекомыми, около 15% от массы тела в день
public class Bird : Animal
{
    private const double RationRatio = 0.15;
    private readonly double _wingSpan;

    //Размах крыльев, м
    public double WingSpan => _wingSpan;

    public Bird(string name, double weight, double wingSpan) : base(name, weight)
    {
        if (wingSpan <= 0)
            throw new ArgumentOutOfRangeException(nameof(wingSpan), "Размах крыльев должен быть положительным.");
        _wingSpan = wingSpan;
    }
    public override string FoodType => "зерно, насекомые";

    public override double DailyFoodKg() => Weight * RationRatio;

    public override string ToString() => $"{base.ToString()}, размах крыльев: {WingSpan:F2} м";
}