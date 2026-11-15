namespace SonarDemo.Tests;

public class CalculatorTests
{
    private readonly Calculator _calc = new();

    [Fact]
    public void Add_ReturnsSum() => Assert.Equal(5, _calc.Add(2, 3));

    [Fact]
    public void Subtract_ReturnsDifference() => Assert.Equal(6, _calc.Subtract(10, 4));

    [Fact]
    public void Multiply_ReturnsProduct() => Assert.Equal(12, _calc.Multiply(3, 4));

    [Fact]
    public void Divide_ReturnsQuotient() => Assert.Equal(5, _calc.Divide(10, 2));

    [Fact]
    public void Divide_ByZero_Throws() =>
        Assert.Throws<DivideByZeroException>(() => _calc.Divide(1, 0));

    [Theory]
    [InlineData(95, "A")]
    [InlineData(85, "B")]
    [InlineData(75, "C")]
    [InlineData(65, "D")]
    [InlineData(10, "F")]
    public void Grade_ReturnsLetter(int score, string expected) =>
        Assert.Equal(expected, Calculator.Grade(score));

    [Fact]
    public void Grade_OutOfRange_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculator.Grade(101));

    [Fact]
    public void Average_ReturnsMean() =>
        Assert.Equal(2.5, Calculator.Average(new double[] { 1, 2, 3, 4 }));

    [Fact]
    public void Average_Empty_Throws() =>
        Assert.Throws<ArgumentException>(() => Calculator.Average(Array.Empty<double>()));
}
