namespace Lab4.Core;

//Алгоритмы из лабораторной работы 1, вынесенные в чистые тестируемые методы
public static class Mathalgorithms
{
    private const double DefaultEpsilon = 1e-6;


    public static long Factorial(int n)
    {
        if (n < 0 || n > 20)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть в диапазоне 0..20.");

        long result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;

        return result;
    }

    public static IReadOnlyList<long> Fibonacci(int count)
    {
        if (count < 0 || count > 40)
            throw new ArgumentOutOfRangeException(nameof(count), "count должно быть в диапазоне 0..40.");

        var result = new List<long>();
        long a = 0, b = 1;

        for (int i = 0; i < count; i++)
        {
            result.Add(a);
            (a, b) = (b, a + b);
        }

        return result;
    }
    //A(x) = sqrt(ln(4/x)) - 1/x - exp(sin(x)).
    //Область определения: 0 &lt; x &lt;= 4 (иначе под корнем отрицательное число)
    public static double Function(double x)
    {
        if (x <= 0 || x > 4)
            throw new ArgumentOutOfRangeException(nameof(x), "x должно быть в диапазоне (0; 4].");

        double ln = Math.Log(4.0 / x);
        double sqrt = Math.Sqrt(ln);
        double division = 1.0 / x;
        double exp = Math.Exp(Math.Sin(x));

        return sqrt - division - exp;
    }

    //Ряд Тейлора для arctg(x), |x| &lt;= 1
    public static double ArcTanTaylor(double x, double epsilon = DefaultEpsilon)
    {
        if (Math.Abs(x) > 1)
            throw new ArgumentOutOfRangeException(nameof(x), "|x| должно быть <= 1.");

        double sum = 0;
        double term = x;
        int n = 1;

        while (Math.Abs(term) > epsilon)
        {
            sum += term;
            term *= -x * x * (2 * n - 1) / (2 * n + 1);
            n++;
        }

        return sum;
    }
}