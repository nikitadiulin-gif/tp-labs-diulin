using Lab4.Core;
using Xunit;

namespace Lab4.Tests;

public class AnimalTests
{
    [Theory]
    [InlineData(100, 3.0)]
    [InlineData(200, 6.0)]
    public void Predator_РассчитываетРацион_РавныйПроцентуОтВеса(double weight, double expectedRation)
    {
        var predator = new Predator("Лев", weight);
        Assert.Equal(expectedRation, predator.DailyFoodKg(), 2);
    }
    [Fact]
    public void Herbivore_РассчитываетРацион_РавныйПроцентуОтВеса()
    {
        var herbivore = new Herbivore("Зебра", 350);
        Assert.Equal(28.0, herbivore.DailyFoodKg(), 2);
    }
    [Fact]
    public void Bird_РассчитываетРацион_РавныйПроцентуОтВеса()
    {
        var bird = new Bird("Орёл", 5.5, 2.1);
        Assert.Equal(0.825, bird.DailyFoodKg(), 3);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Animal_НекорректныйВес_БросаетИсключение(double weight)
    {
        //защита инварианта: вес не может быть нулевым или отрицательным
        Assert.Throws<ArgumentOutOfRangeException>(() => new Predator("Тест", weight));
    }
    [Fact]
    public void Bird_НекорректныйРазмахКрыльев_БросаетИсключение()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bird("Тест", 1.0, -0.5));
    }
    [Fact]
    public void Animal_ПустоеИмя_БросаетИсключение()
    {
        Assert.Throws<ArgumentException>(() => new Predator("", 100));
    }
    [Fact]
    public void Bird_ToString_СодержитРазмахКрыльев()
    {
        var bird = new Bird("Попугай", 0.4, 0.5);
        string text = bird.ToString();
        Assert.Contains("размах крыльев", text);
    }
}