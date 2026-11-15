namespace SonarDemo.Tests;

public class StringUtilsTests
{
    [Fact]
    public void Reverse_ReversesText() => Assert.Equal("cba", StringUtils.Reverse("abc"));

    [Fact]
    public void IsPalindrome_DetectsPalindromes()
    {
        Assert.True(StringUtils.IsPalindrome("A man, a plan, a canal: Panama"));
        Assert.False(StringUtils.IsPalindrome("hello"));
    }

    [Fact]
    public void CountVowels_CountsCorrectly() => Assert.Equal(4, StringUtils.CountVowels("SonarQube"));
}
