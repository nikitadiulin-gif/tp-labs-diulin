using Lab4.Core;
using Xunit;

namespace Lab4.Tests;

public class MathAlgorithmsTests
{
    //Factorial
    [Fact]
    public void Factorial_ОтНуля_ВозвращаетЕдиницу()
    {
        // Arrange + Act
        long result = Mathalgorithms.Factorial(0);
        // Assert
        Assert.Equal(1, result);
    }
    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    [InlineData(20, 2432902008176640000)] // граничное значение: максимум, не переполняющий long
    public void Factorial_КорректныеЗначения_ВозвращаетОжидаемыйРезультат(int n, long expected)
    {
        long result = Mathalgorithms.Factorial(n);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Factorial_ВыходЗаДиапазон_БросаетИсключение(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mathalgorithms.Factorial(n));
    }
    //Fibonacci
    [Fact]
    public void Fibonacci_НулевоеКоличество_ВозвращаетПустойСписок()
    {
        var result = Mathalgorithms.Fibonacci(0);
        Assert.Empty(result);
    }
    [Theory]
    [InlineData(1, new long[] { 0 })]
    [InlineData(2, new long[] { 0, 1 })]
    [InlineData(6, new long[] { 0, 1, 1, 2, 3, 5 })]
    public void Fibonacci_КорректноеКоличество_ВозвращаетОжидаемуюПоследовательность(int count, long[] expected)
    {
        var result = Mathalgorithms.Fibonacci(count);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Fibonacci_МаксимальноеКоличество_ВозвращаетСписокНужнойДлины()
    {
        var result = Mathalgorithms.Fibonacci(40);
        Assert.Equal(40, result.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(41)]
    public void Fibonacci_ВыходЗаДиапазон_БросаетИсключение(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mathalgorithms.Fibonacci(count));
    }
    //Function: A(x) = sqrt(ln(4/x)) - 1/x - exp(sin(x))
    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.5)]
    public void Function_КорректныйАргумент_СовпадаетСРучнымВычислением(double x)
    {
        double expected = Math.Sqrt(Math.Log(4.0 / x)) - 1.0 / x - Math.Exp(Math.Sin(x));
        double actual = Mathalgorithms.Function(x);
        Assert.Equal(expected, actual, 10);
    }
    [Fact]
    public void Function_НаГраницеОбластиОпределения_ВозвращаетКорректныйРезультат()
    {
        // при x = 4: ln(4/4) = ln(1) = 0, sqrt(0) = 0
        double result = Mathalgorithms.Function(4.0);
        double expected = 0.0 - 1.0 / 4.0 - Math.Exp(Math.Sin(4.0));
        Assert.Equal(expected, result, 10);
    }
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(5.0)]
    public void Function_ВнеОбластиОпределения_БросаетИсключение(double x)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mathalgorithms.Function(x));
    }
    //ArcTanTaylor
    [Fact]
    public void ArcTanTaylor_ОтНуля_ВозвращаетНоль()
    {
        double result = Mathalgorithms.ArcTanTaylor(0.0);
        Assert.Equal(0.0, result, 10);
    }
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]  // граница области определения
    [InlineData(-1.0)] // граница области определения
    public void ArcTanTaylor_ВГраницахОбласти_СовпадаетСMathAtan(double x)
    {
        double result = Mathalgorithms.ArcTanTaylor(x);
        Assert.Equal(Math.Atan(x), result, 5);
    }
    [Theory]
    [InlineData(1.5)]
    [InlineData(-2.0)]
    public void ArcTanTaylor_ВнеОбластиОпределения_БросаетИсключение(double x)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mathalgorithms.ArcTanTaylor(x));
    }
}