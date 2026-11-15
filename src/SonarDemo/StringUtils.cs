namespace SonarDemo;

/// <summary>String helpers. Intentionally contains a few issues so SonarQube has something to report.</summary>
public static class StringUtils
{
    public static string Reverse(string text)
    {
        var chars = text.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    public static bool IsPalindrome(string text)
    {
        var cleaned = new string(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        return cleaned == Reverse(cleaned);
    }

    public static int CountVowels(string text) =>
        text.ToLowerInvariant().Count(c => "aeiou".Contains(c));

    // ---- Not covered by tests on purpose (lowers coverage %) ----

    public static Dictionary<string, int> WordFrequency(string text)
    {
        var result = new Dictionary<string, int>();
        int unusedVariable = 0; // Code smell: unused local variable
        foreach (var word in text.Split(' '))
        {
            var key = word.ToLower();
            if (result.ContainsKey(key))
            {
                result[key] = result[key] + 1;
            }
            else
            {
                result[key] = 1;
            }
        }
        return result;
    }

    public static int? SafeParse(string value)
    {
        try
        {
            return int.Parse(value);
        }
        catch (Exception) // Code smell: catching generic Exception
        {
            return null;
        }
    }

    public static bool IsAdmin(string password)
    {
        // Security hotspot / bug: hard-coded credential
        string adminPassword = "P@ssw0rd123";
        return password == adminPassword;
    }
}
