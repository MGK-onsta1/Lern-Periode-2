using System.Runtime.InteropServices;

namespace Morse
{
    /// <summary>
    /// Erzeugt Morse-Töne und spielt sie über die Windows-Funktion waveOut ab.
    /// (SoundPlayer wurde ersetzt, weil dessen Stop() bis zu 1 Sekunde blockiert –
    ///  dadurch kamen bei der Leertasten-Eingabe die Tastendrücke zu spät an.)
    /// Timing nach PARIS-Norm: 1 Einheit = 1200 / WpM Millisekunden.
    ///   Punkt = 1 Einheit, Strich = 3 Einheiten,
    ///   Pause zwischen Zeichen-Elementen = 1, zwischen Buchstaben = 3, zwischen Wörtern = 7.
    /// </summary>
    public sealed class MorseAudio : IDisposable
    {
        private const int SampleRate = 22050;
        private const double Volume = 0.5;
        private const int RampMs = 5; // weiches Ein-/Ausblenden gegen Knackgeräusche

        private readonly WaveOut _out = new(SampleRate);
        private short[]? _loopTone;
        private int _loopToneFreq;

        /// <summary>Tonhöhe in Hertz.</summary>
        public int Frequency { get; set; } = 600;

        /// <summary>Tempo in Wörtern pro Minute.</summary>
        public int Wpm { get; set; } = 12;

        /// <summary>Länge einer Einheit (= ein Punkt) in Millisekunden.</summary>
        public double UnitMs => 1200.0 / Math.Max(1, Wpm);

        /// <summary>Spielt einen Morse-String (". - / ") im Hintergrund ab.</summary>
        public void Play(string morse) => _out.Play(BuildSamples(MorseTable.Normalize(morse)), loop: false);

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
            _out.Play(_loopTone, loop: true);
        }

        /// <summary>Beendet den Dauerton bzw. jede laufende Wiedergabe – sofort.</summary>
        public void Stop() => _out.Stop();

        /// <summary>Berechnet die Dauer eines Morse-Strings in Millisekunden.</summary>
        public double DurationMs(string morse)
        {
            int units = 0;
            foreach (var (_, u) in Elements(MorseTable.Normalize(morse)))
                units += u;
            return units * UnitMs;
        }

        // ---------------------------------------------------------------- Töne berechnen

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

        private short[] BuildSamples(string morse)
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
            return samples.ToArray();
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
        private short[] BuildLoopTone()
        {
            int length = SampleRate / 2;                       // 0,5 s
            int cycles = Math.Max(1, (int)Math.Round(Frequency * 0.5));
            var s = new short[length];
            for (int i = 0; i < length; i++)
                s[i] = (short)(Math.Sin(2 * Math.PI * cycles * i / length) * Volume * short.MaxValue);
            return s;
        }

        public void Dispose() => _out.Dispose();

        // ---------------------------------------------------------------- Windows-Audio (waveOut)

        /// <summary>
        /// Sehr kleiner Wrapper um die Windows-Funktionen waveOut* (winmm.dll).
        /// Spielt 16-Bit-Mono-Samples ab, optional in Endlosschleife; Stop() wirkt sofort.
        /// </summary>
        private sealed class WaveOut : IDisposable
        {
            private readonly int _sampleRate;
            private IntPtr _device;      // geöffnetes Audiogerät
            private IntPtr _header;      // WAVEHDR im nicht verwalteten Speicher
            private IntPtr _data;        // Sample-Daten im nicht verwalteten Speicher

            public WaveOut(int sampleRate)
            {
                _sampleRate = sampleRate;
                // Das erste Öffnen dauert ca. 0,5 s – deshalb gleich beim Start,
                // nicht erst beim ersten Druck auf die Leertaste.
                EnsureOpen();
                // Einmal kurz Stille abspielen: Auch die erste Wiedergabe ist langsam.
                Play(new short[sampleRate / 20], loop: false);
            }

            public void Play(short[] samples, bool loop)
            {
                Stop();
                if (samples.Length == 0 || !EnsureOpen()) return;

                int bytes = samples.Length * 2;
                _data = Marshal.AllocHGlobal(bytes);
                Marshal.Copy(samples, 0, _data, samples.Length);

                var hdr = new WAVEHDR
                {
                    lpData = _data,
                    dwBufferLength = (uint)bytes,
                    dwFlags = loop ? WHDR_BEGINLOOP | WHDR_ENDLOOP : 0,
                    dwLoops = loop ? uint.MaxValue : 0,
                };
                _header = Marshal.AllocHGlobal(Marshal.SizeOf<WAVEHDR>());
                Marshal.StructureToPtr(hdr, _header, false);

                int size = Marshal.SizeOf<WAVEHDR>();
                if (waveOutPrepareHeader(_device, _header, size) != 0 ||
                    waveOutWrite(_device, _header, size) != 0)
                {
                    Stop();
                }
            }

            public void Stop()
            {
                if (_device != IntPtr.Zero) waveOutReset(_device); // bricht sofort ab
                if (_header != IntPtr.Zero)
                {
                    waveOutUnprepareHeader(_device, _header, Marshal.SizeOf<WAVEHDR>());
                    Marshal.FreeHGlobal(_header);
                    _header = IntPtr.Zero;
                }
                if (_data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_data);
                    _data = IntPtr.Zero;
                }
            }

            private bool EnsureOpen()
            {
                if (_device != IntPtr.Zero) return true;
                var fmt = new WAVEFORMATEX
                {
                    wFormatTag = 1,                       // PCM
                    nChannels = 1,                        // Mono
                    nSamplesPerSec = (uint)_sampleRate,
                    wBitsPerSample = 16,
                    nBlockAlign = 2,
                    nAvgBytesPerSec = (uint)_sampleRate * 2,
                };
                // Kein Audiogerät vorhanden → einfach stumm bleiben
                return waveOutOpen(out _device, WAVE_MAPPER, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) == 0;
            }

            public void Dispose()
            {
                Stop();
                if (_device != IntPtr.Zero)
                {
                    waveOutClose(_device);
                    _device = IntPtr.Zero;
                }
            }

            private const int WAVE_MAPPER = -1;
            private const uint WHDR_BEGINLOOP = 0x04;
            private const uint WHDR_ENDLOOP = 0x08;

            [StructLayout(LayoutKind.Sequential)]
            private struct WAVEFORMATEX
            {
                public ushort wFormatTag;
                public ushort nChannels;
                public uint nSamplesPerSec;
                public uint nAvgBytesPerSec;
                public ushort nBlockAlign;
                public ushort wBitsPerSample;
                public ushort cbSize;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WAVEHDR
            {
                public IntPtr lpData;
                public uint dwBufferLength;
                public uint dwBytesRecorded;
                public IntPtr dwUser;
                public uint dwFlags;
                public uint dwLoops;
                public IntPtr lpNext;
                public IntPtr reserved;
            }

            [DllImport("winmm.dll")] private static extern int waveOutOpen(out IntPtr hwo, int uDeviceID, ref WAVEFORMATEX pwfx, IntPtr dwCallback, IntPtr dwInstance, int fdwOpen);
            [DllImport("winmm.dll")] private static extern int waveOutPrepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
            [DllImport("winmm.dll")] private static extern int waveOutUnprepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
            [DllImport("winmm.dll")] private static extern int waveOutWrite(IntPtr hwo, IntPtr pwh, int cbwh);
            [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr hwo);
            [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr hwo);
        }
    }
}
