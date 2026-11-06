using System.Media;

namespace Morse
{
    /// <summary>
    /// Erzeugt Morse-Töne als WAV im Speicher und spielt sie mit SoundPlayer ab.
    /// Timing nach PARIS-Norm: 1 Einheit = 1200 / WpM Millisekunden.
    ///   Punkt = 1 Einheit, Strich = 3 Einheiten,
    ///   Pause zwischen Zeichen-Elementen = 1, zwischen Buchstaben = 3, zwischen Wörtern = 7.
    /// </summary>
    public sealed class MorseAudio : IDisposable
    {
        private const int SampleRate = 22050;
        private const double Volume = 0.5;
        private const int RampMs = 5; // weiches Ein-/Ausblenden gegen Knackgeräusche

        private SoundPlayer? _player;
        private MemoryStream? _stream;
        private byte[]? _loopTone;
        private int _loopToneFreq;

        /// <summary>Tonhöhe in Hertz.</summary>
        public int Frequency { get; set; } = 600;

        /// <summary>Tempo in Wörtern pro Minute.</summary>
        public int Wpm { get; set; } = 12;

        /// <summary>Länge einer Einheit (= ein Punkt) in Millisekunden.</summary>
        public double UnitMs => 1200.0 / Math.Max(1, Wpm);

        /// <summary>Spielt einen Morse-String (". - / ") im Hintergrund ab.</summary>
        public void Play(string morse)
        {
            byte[] wav = BuildWav(MorseTable.Normalize(morse));
            PlayBytes(wav, loop: false);
        }

        /// <summary>Spielt einen einzelnen Punkt oder Strich.</summary>
        public void PlaySymbol(char symbol) => Play(symbol.ToString());

        /// <summary>Startet einen Dauerton (für die Leertasten-Eingabe).</summary>
        public void StartTone()
        {
            if (_loopTone == null || _loopToneFreq != Frequency)
            {
                _loopTone = BuildLoopTone();
                _loopToneFreq = Frequency;
            }
            PlayBytes(_loopTone, loop: true);
        }

        /// <summary>Beendet den Dauerton bzw. jede laufende Wiedergabe.</summary>
        public void Stop()
        {
            _player?.Stop();
        }

        /// <summary>Berechnet die Dauer eines Morse-Strings in Millisekunden.</summary>
        public double DurationMs(string morse)
        {
            int units = 0;
            foreach (var (isTone, u) in Elements(MorseTable.Normalize(morse)))
                units += u;
            return units * UnitMs;
        }

        // ---------------------------------------------------------------- intern

        private void PlayBytes(byte[] wav, bool loop)
        {
            _player?.Stop();
            _player?.Dispose();
            _stream?.Dispose();

            _stream = new MemoryStream(wav);
            _player = new SoundPlayer(_stream);
            _player.Load();
            if (loop) _player.PlayLooping();
            else _player.Play();
        }

        /// <summary>Zerlegt den Morse-String in (Ton?, Einheiten)-Abschnitte.</summary>
        private static IEnumerable<(bool isTone, int units)> Elements(string morse)
        {
            bool first = true;
            int pendingGap = 1;
            foreach (char c in morse)
            {
                switch (c)
                {
                    case '.':
                    case '-':
                        if (!first) yield return (false, pendingGap);
                        yield return (true, c == '.' ? 1 : 3);
                        first = false;
                        pendingGap = 1;
                        break;
                    case ' ':
                        pendingGap = Math.Max(pendingGap, 3);
                        break;
                    case '/':
                        pendingGap = 7;
                        break;
                }
            }
        }

        private byte[] BuildWav(string morse)
        {
            var samples = new List<short>();
            int samplesPerUnit = (int)(SampleRate * UnitMs / 1000.0);

            // kurze Stille am Anfang, damit der erste Ton nicht abgeschnitten wird
            samples.AddRange(new short[SampleRate / 50]);

            foreach (var (isTone, units) in Elements(morse))
            {
                int n = samplesPerUnit * units;
                if (isTone) AppendTone(samples, n);
                else samples.AddRange(new short[n]);
            }
            samples.AddRange(new short[SampleRate / 50]);
            return ToWav(samples.ToArray());
        }

        private void AppendTone(List<short> samples, int count)
        {
            int ramp = Math.Min(count / 2, SampleRate * RampMs / 1000);
            for (int i = 0; i < count; i++)
            {
                double env = 1.0;
                if (i < ramp) env = (double)i / ramp;
                else if (i > count - ramp) env = (double)(count - i) / ramp;

                double v = Math.Sin(2 * Math.PI * Frequency * i / SampleRate) * Volume * env;
                samples.Add((short)(v * short.MaxValue));
            }
        }

        /// <summary>
        /// Ein Stück Ton, das sich nahtlos wiederholen lässt: Es enthält genau eine ganze
        /// Anzahl an Schwingungen, damit beim Loopen kein Knacken entsteht.
        /// </summary>
        private byte[] BuildLoopTone()
        {
            int length = SampleRate / 2;                       // 0,5 s
            int cycles = Math.Max(1, (int)Math.Round(Frequency * 0.5));
            var s = new short[length];
            for (int i = 0; i < length; i++)
                s[i] = (short)(Math.Sin(2 * Math.PI * cycles * i / length) * Volume * short.MaxValue);
            return ToWav(s);
        }

        /// <summary>Verpackt 16-Bit-Mono-Samples in eine WAV-Datei (RIFF).</summary>
        private static byte[] ToWav(short[] samples)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            int dataSize = samples.Length * 2;

            w.Write("RIFF"u8.ToArray());
            w.Write(36 + dataSize);
            w.Write("WAVE"u8.ToArray());
            w.Write("fmt "u8.ToArray());
            w.Write(16);                 // Größe des fmt-Blocks
            w.Write((short)1);           // PCM
            w.Write((short)1);           // Mono
            w.Write(SampleRate);
            w.Write(SampleRate * 2);     // Bytes pro Sekunde
            w.Write((short)2);           // Block-Align
            w.Write((short)16);          // Bits pro Sample
            w.Write("data"u8.ToArray());
            w.Write(dataSize);
            foreach (short v in samples) w.Write(v);

            w.Flush();
            return ms.ToArray();
        }

        public void Dispose()
        {
            _player?.Stop();
            _player?.Dispose();
            _stream?.Dispose();
        }
    }
}
