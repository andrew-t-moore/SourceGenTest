namespace MakeEnumsGreatAgain.Generators;

public static class CamelCase
{
    public static string ToCamelCase(string input)
    {
        // TODO: make this not suck.
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new Exception("Input string was null or whitespace");
        }

        var prefix = new string(input
                .TakeWhile(char.IsUpper)
                .ToArray())
            .ToLowerInvariant();

        return prefix.Length == input.Length
            ? prefix
            : prefix + input.Substring(prefix.Length);
    }
}