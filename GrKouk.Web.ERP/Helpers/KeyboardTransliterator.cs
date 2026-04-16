using System.Collections.Generic;
using System.Text;

namespace GrKouk.Web.ERP.Helpers;

/// <summary>
/// Maps characters between Greek and English keyboard positions (key-position based, not phonetic).
/// </summary>
public static class KeyboardTransliterator
{
    private static readonly Dictionary<char, char> EnglishToGreek = new()
    {
        ['a'] = 'α', ['b'] = 'β', ['c'] = 'ψ', ['d'] = 'δ', ['e'] = 'ε',
        ['f'] = 'φ', ['g'] = 'γ', ['h'] = 'η', ['i'] = 'ι', ['j'] = 'ξ',
        ['k'] = 'κ', ['l'] = 'λ', ['m'] = 'μ', ['n'] = 'ν', ['o'] = 'ο',
        ['p'] = 'π', ['q'] = ';', ['r'] = 'ρ', ['s'] = 'σ', ['t'] = 'τ',
        ['u'] = 'θ', ['v'] = 'ω', ['w'] = 'ς', ['x'] = 'χ', ['y'] = 'υ',
        ['z'] = 'ζ',
    };

    private static readonly Dictionary<char, char> GreekToEnglish = new();

    static KeyboardTransliterator()
    {
        foreach (var kvp in EnglishToGreek)
        {
            GreekToEnglish[kvp.Value] = kvp.Key;
        }
    }

    public static string Transliterate(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            var lower = char.ToLower(ch);
            if (EnglishToGreek.TryGetValue(lower, out var greek))
            {
                sb.Append(char.IsUpper(ch) ? char.ToUpper(greek) : greek);
            }
            else if (GreekToEnglish.TryGetValue(lower, out var english))
            {
                sb.Append(char.IsUpper(ch) ? char.ToUpper(english) : english);
            }
            else
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }
}
