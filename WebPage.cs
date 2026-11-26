using System.Reflection;
using System.Text.Json;

namespace Morse
{
    /// <summary>Die Webseite für Browser-Freunde (web/index.html, ins Programm eingebettet).</summary>
    public static class WebPage
    {
        private static readonly string Template = Load();

        /// <summary>Pixel-Schrift für die Webseite – kommt vom Programm, damit sie auch ohne Internet geht.</summary>
        public static readonly byte[] PixelFont = LoadBytes("Morse.fonts.PressStart2P.ttf");

        private static string Load()
        {
            using var reader = new StreamReader(Open("Morse.web.index.html"));
            // Die Morse-Tabelle kommt aus MorseTable.cs, damit Programm und Webseite immer gleich sind.
            var codes = JsonSerializer.Serialize(MorseTable.Codes.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
            return reader.ReadToEnd().Replace("__MORSE_CODES__", codes);
        }

        /// <summary>Seite mit Standard-Farbe (Freunde können die Farbe im Browser selbst ändern).</summary>
        public static string Render()
        {
            var theme = JsonSerializer.Serialize(new { name = "Blau", accent = "#3b82f6", dark = true });
            return Template.Replace("__THEME__", theme);
        }

        private static byte[] LoadBytes(string name)
        {
            using var stream = Open(name);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }

        private static Stream Open(string name) =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException(name + " fehlt im Programm.");
    }
}
