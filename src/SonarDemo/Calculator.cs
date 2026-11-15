namespace SonarDemo;

/// <summary>Basic arithmetic and grading logic used to demo SonarQube analysis.</summary>
public class Calculator
{
    public double Add(double a, double b) => a + b;

    public double Subtract(double a, double b) => a - b;

    public double Multiply(double a, double b) => a * b;

    public double Divide(double a, double b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero");
        }
        return a / b;
    }

    public static string Grade(int score)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 0 and 100");
        }

        return score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }

    public static double Average(IReadOnlyCollection<double> numbers)
    {
        if (numbers.Count == 0)
        {
            throw new ArgumentException("List must not be empty", nameof(numbers));
        }
        return numbers.Sum() / numbers.Count;
    }
}
