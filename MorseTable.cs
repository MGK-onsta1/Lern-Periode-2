using System.Text;

namespace Morse
{
    /// <summary>
    /// Übersetzt zwischen Text und Morsecode.
    /// Format: Buchstaben durch ein Leerzeichen getrennt, Wörter durch " / ".
    /// </summary>
    public static class MorseTable
    {
        public static readonly IReadOnlyDictionary<char, string> Codes = new Dictionary<char, string>
        {
            ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".",
            ['F'] = "..-.", ['G'] = "--.", ['H'] = "....", ['I'] = "..", ['J'] = ".---",
            ['K'] = "-.-", ['L'] = ".-..", ['M'] = "--", ['N'] = "-.", ['O'] = "---",
            ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-",
            ['U'] = "..-", ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-", ['Y'] = "-.--",
            ['Z'] = "--..",
            ['0'] = "-----", ['1'] = ".----", ['2'] = "..---", ['3'] = "...--", ['4'] = "....-",
            ['5'] = ".....", ['6'] = "-....", ['7'] = "--...", ['8'] = "---..", ['9'] = "----.",
            ['Ä'] = ".-.-", ['Ö'] = "---.", ['Ü'] = "..--", ['ß'] = "...--..",
            ['.'] = ".-.-.-", [','] = "--..--", ['?'] = "..--..", ['!'] = "-.-.--", ['\''] = ".----.",
            ['/'] = "-..-.", ['('] = "-.--.", [')'] = "-.--.-", ['&'] = ".-...", [':'] = "---...",
            [';'] = "-.-.-.", ['='] = "-...-", ['+'] = ".-.-.", ['-'] = "-....-", ['_'] = "..--.-",
            ['"'] = ".-..-.", ['@'] = ".--.-.",
        };

        /// <summary>Umgekehrte Tabelle: Morsecode → Zeichen.</summary>
        public static readonly IReadOnlyDictionary<string, char> Reverse =
            Codes.ToDictionary(kv => kv.Value, kv => kv.Key);

        /// <summary>
        /// Text → Morse. Unbekannte Zeichen werden übersprungen und in <paramref name="unknown"/> gesammelt.
        /// </summary>
        public static string TextToMorse(string text, out string unknown)
        {
            var words = new List<string>();
            var skipped = new StringBuilder();

            foreach (string word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var letters = new List<string>();
                foreach (char raw in word)
                {
                    // 'ß' hat keinen Großbuchstaben, alles andere wird groß geschrieben.
                    char c = raw == 'ß' ? raw : char.ToUpperInvariant(raw);
                    if (Codes.TryGetValue(c, out string? code))
                        letters.Add(code);
                    else if (skipped.ToString().IndexOf(raw) < 0)
                        skipped.Append(raw);
                }
                if (letters.Count > 0)
                    words.Add(string.Join(" ", letters));
            }

            unknown = skipped.ToString();
            return string.Join(" / ", words);
        }

        public static string TextToMorse(string text) => TextToMorse(text, out _);

        /// <summary>
        /// Morse → Text. Unbekannte Codes werden als '?' ausgegeben und in <paramref name="unknownCount"/> gezählt.
        /// Akzeptiert auch · • − – — als Punkt/Strich und "|" oder mehrere Leerzeichen als Worttrenner.
        /// </summary>
        public static string MorseToText(string morse, out int unknownCount)
        {
            unknownCount = 0;
            string normalized = Normalize(morse);
            var sb = new StringBuilder();

            string[] words = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int w = 0; w < words.Length; w++)
            {
                if (w > 0) sb.Append(' ');
                foreach (string code in words[w].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Reverse.TryGetValue(code, out char c))
                        sb.Append(c);
                    else
                    {
                        sb.Append('?');
                        unknownCount++;
                    }
                }
            }
            return sb.ToString();
        }

        public static string MorseToText(string morse) => MorseToText(morse, out _);

        /// <summary>Prüft, ob die Eingabe wie Morsecode aussieht (nur Punkte, Striche, Trenner).</summary>
        public static bool LooksLikeMorse(string input)
        {
            string n = Normalize(input);
            bool hasSymbol = false;
            foreach (char c in n)
            {
                if (c == '.' || c == '-') hasSymbol = true;
                else if (c != ' ' && c != '/') return false;
            }
            return hasSymbol;
        }

        /// <summary>Ersetzt typografische Varianten durch '.', '-' und '/'.</summary>
        public static string Normalize(string morse)
        {
            var sb = new StringBuilder(morse.Length);
            foreach (char c in morse.Replace("\r", " ").Replace("\n", " / ").Replace("\t", " "))
            {
                sb.Append(c switch
                {
                    '·' or '•' or '∙' => '.',
                    '−' or '–' or '—' => '-',
                    '|' => '/',
                    _ => c
                });
            }
            // Drei oder mehr Leerzeichen hintereinander gelten auch als Wortgrenze.
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), " {3,}", " / ").Trim();
        }

        /// <summary>Hübsche Darstellung: · und − statt . und -.</summary>
        public static string Pretty(string morse) => morse.Replace('.', '·').Replace('-', '−');
    }
}
