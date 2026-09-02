namespace Waystone.Monads.Schemas;

internal static class EmailAddress
{
    private const int MaxLength = 254;

    private const int MaxLocalLength = 64;

    internal static bool IsWellFormed(string value)
    {
        if (value.Length == 0 || value.Length > MaxLength) return false;

        int at = value.LastIndexOf('@');

        if (at <= 0 || at == value.Length - 1) return false;

        return IsLocalPart(value, at) && IsDomain(value, at + 1);
    }

    private static bool IsLocalPart(string value, int end)
    {
        if (end > MaxLocalLength) return false;

        if (value[end - 1] == '.') return false;

        if (!IsLocalCharacter(value[0]) || value[0] == '.') return false;

        for (int index = 1; index < end; index++)
        {
            char character = value[index];

            if (!IsLocalCharacter(character)) return false;

            if (character == '.' && value[index - 1] == '.') return false;
        }

        return true;
    }

    private static bool IsDomain(string value, int start)
    {
        char last = value[value.Length - 1];

        if (last == '.' || last == '-') return false;

        char first = value[start];

        if (!IsDomainCharacter(first) || first == '.' || first == '-')
        {
            return false;
        }

        for (int index = start + 1; index < value.Length; index++)
        {
            char character = value[index];

            if (!IsDomainCharacter(character)) return false;

            char previous = value[index - 1];

            if (character == '.' && (previous == '.' || previous == '-'))
            {
                return false;
            }

            if (character == '-' && previous == '.') return false;
        }

        return true;
    }

    private static bool IsLocalCharacter(char character) =>
        IsLetterOrDigit(character)
     || character == '.'
     || character == '!'
     || character == '#'
     || character == '$'
     || character == '%'
     || character == '&'
     || character == '\''
     || character == '*'
     || character == '+'
     || character == '-'
     || character == '/'
     || character == '='
     || character == '?'
     || character == '^'
     || character == '_'
     || character == '`'
     || character == '{'
     || character == '|'
     || character == '}'
     || character == '~';

    private static bool IsDomainCharacter(char character) =>
        IsLetterOrDigit(character) || character == '.' || character == '-';

    private static bool IsLetterOrDigit(char character) =>
        (character >= 'a' && character <= 'z')
     || (character >= 'A' && character <= 'Z')
     || (character >= '0' && character <= '9');
}
