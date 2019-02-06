﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
namespace WaveGenerator
{
    public class SoundGenerator
    {
        private uint _generatedSampleCount = 0;
        private WaveFile _waveFile;
        private double _lastPhase = 0;
        private double[] _lastPhases = null;
        private double _soundAmplitude = 1;

        /// <summary>
        /// The volume of sounds, can only be in range from 0 to 1. Any inappropriate value is corrected to 1.
        /// </summary>
        public double Volume
        {
            get { return _soundAmplitude; }
            set
            {
                if (value >= 0 && value <= 1)
                    _soundAmplitude = value;
                else
                    _soundAmplitude = 1;
            }
        }
        /// <summary>
        /// The duration of generated sounds.
        /// </summary>
        public TimeSpan GeneratedLength
        {
            get
            {
                var ms = (double)_generatedSampleCount / _waveFile.SampleRate * 1000;
                return new TimeSpan(0, 0, 0, 0, (int)ms);
            }
        }

        public SoundGenerator(WaveFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            else
                _waveFile = file;
        }
        public SoundGenerator()
        {
            _waveFile = new WaveFile();
        }
        /// <summary>
        /// Generates a simple sine wave at the end of the file.
        /// </summary>
        /// <param name="frequency">Frequency of the wave</param>
        /// <param name="duration">Duration of the wave</param>
        /// <param name="fade">Should the wave fade in and out? Default is true</param>
        /// <exception cref="System.ArgumentException">Occurs when frequency or duration is less than zero.</exception>
        public void AddSimpleTone(double frequency, double duration, bool fade = true)
        {
            if (duration == 0)
            {
                this._lastPhase = 0;
                return;
            }
            if (duration < 0)
                throw new ArgumentException("Duration cannot be negative.", nameof(duration));
            if (frequency < 0)
                throw new ArgumentException("Frequency cannot be negative.", nameof(frequency));

            var sampleCount = (uint)Math.Floor(duration * _waveFile.SampleRate / 1000);
            var fileAmplitude = (Math.Pow(2, (byte)_waveFile.BitDepth - 1) - 1) * _soundAmplitude;
            var radPerSample = 2 * Math.PI / _waveFile.SampleRate;

            var fadeLen = (uint)(sampleCount * 0.10);
            var fadeInAmp = Fade(0, 1, fadeLen).GetEnumerator();
            var fadeOutAmp = Fade(1, 0, fadeLen).GetEnumerator();
            var index = 0;
            var channels = (byte)_waveFile.Channels;

            foreach (var sample in GenerateSineWave(frequency, sampleCount, radPerSample, this._lastPhase))
            {
                var sin = fileAmplitude * sample;
                if (fade)
                {
                    if (index < fadeLen && fadeInAmp.MoveNext())
                        sin *= fadeInAmp.Current;
                    if (index >= sampleCount - fadeLen && fadeOutAmp.MoveNext())
                        sin *= fadeOutAmp.Current;
                }
                var sinBytes = ConvertNumber((long)sin, (byte)_waveFile.BitDepth);

                for (byte channel = 0; channel < channels; channel++)
                    _waveFile.AddSampleToEnd(sinBytes);

                index++;
            }
            double lastPhase = 0;
            if (!fade)
                lastPhase = GetPhase(radPerSample * sampleCount * frequency + this._lastPhase);
            fadeInAmp.Dispose(); fadeOutAmp.Dispose();
            this._lastPhase = lastPhase;
            this._generatedSampleCount += sampleCount;
        }

        private IEnumerable<double> Fade(double startAmplitude, double endAmplitude, uint length)
        {
            if (startAmplitude < 0 || endAmplitude < 0)
                throw new ArgumentException("The amplitude can't be negative");
            var difference = Math.Abs(startAmplitude - endAmplitude);
            double direction = startAmplitude > endAmplitude ? -1 : 1;
            for (var i = 0; i < length; i++)
                yield return startAmplitude + direction * (difference / length * i);
        }
        /// <summary>
        /// Generates a complex sine wave at the end of the file by combining two or more simple sine waves
        /// </summary>
        /// <param name="duration">Duration of the wave</param>
        /// <param name="fade">Should the wave fade in and out?</param>
        /// <param name="frequencies">Frequencies of simple sine waves which will be combined into a complex wave</param>
        public void AddComplexTone(double duration, bool fade, params double[] frequencies)
        {
            if (duration == 0 ||
                double.IsInfinity(duration) ||
                double.IsNaN(duration) ||
                frequencies == null)
            {
                return;
            }
            var nonZero = frequencies.Count(f => f > 0);
            var sampleCount = (uint)Math.Floor(duration * _waveFile.SampleRate / 1000);
            this._generatedSampleCount += sampleCount;
            var lastPhases = new double[frequencies.Length];
            if (this._lastPhases == null)
                this._lastPhases = lastPhases;
            var fileAmplitude = Math.Pow(2, (byte)_waveFile.BitDepth - 1) - 1;
            var radPerSample = 2 * Math.PI / _waveFile.SampleRate;
            var complexWave = new double[sampleCount];

            var waves = new IEnumerator<double>[frequencies.Length];

            for (var frequencyN = 0; frequencyN < frequencies.Length; frequencyN++)
                waves[frequencyN] = GenerateSineWave(frequencies[frequencyN], sampleCount, radPerSample, this._lastPhases[frequencyN]).GetEnumerator();

            uint index = 0;
            var fadeLen = (uint)(sampleCount * 0.10);
            var fadeInAmp = Fade(0, 1, fadeLen).GetEnumerator();
            var fadeOutAmp = Fade(1, 0, fadeLen).GetEnumerator();
            var channels = (byte)_waveFile.Channels;
            while (waves[0].MoveNext())
            {
                var sin = waves[0].Current;
                for (var i = 1; i < frequencies.Length; i++)
                {
                    waves[i].MoveNext();
                    sin += waves[i].Current;
                }
                sin /= nonZero;
                sin *= fileAmplitude * this._soundAmplitude;
                if (fade)
                {
                    if (index < fadeLen && fadeInAmp.MoveNext())
                        sin *= fadeInAmp.Current;
                    if (index >= sampleCount - fadeLen && fadeOutAmp.MoveNext())
                        sin *= fadeOutAmp.Current;
                }
                var sinBytes = ConvertNumber((long)sin, (byte)_waveFile.BitDepth);

                for (byte channel = 0; channel < channels; channel++)
                    _waveFile.AddSampleToEnd(sinBytes);
                index++;
            }
            if (!fade)
                for (var i = 0; i < frequencies.Length; i++)
                {
                    lastPhases[i] = GetPhase(sampleCount * radPerSample * frequencies[i] + this._lastPhases[i]);
                    waves[i].Dispose();
                }
            this._lastPhases = lastPhases;
        }

        private IEnumerable<double> GenerateSineWave(double frequency, uint length, double xInc, double startPhase)
        {
            for (var x = 0; x < length; x++)
                yield return Math.Sin(frequency * xInc * x + startPhase);
        }

        private double[] GenerateSquareWave(double frequency, int length, double xInc, double startPhase)
        {
            var squareSineWave = new double[length];
            for (var i = 0; i < squareSineWave.Length; i++)
            {
                squareSineWave[i] = Math.Sign(Math.Sin(frequency * xInc * i + startPhase));
            }
            return squareSineWave;
        }

        public void AddSineChirp(double frequency1, double frequency2, double duration)
        {
            var sampleCount = (uint)Math.Floor(duration * _waveFile.SampleRate / 1000);
            var fileAmplitude = (Math.Pow(2, (byte)_waveFile.BitDepth - 1) - 1) * _soundAmplitude;
            var radPerSample = 2 * Math.PI / _waveFile.SampleRate;
            var chirp = GenerateSineChirp(frequency1, frequency2, (int)sampleCount, radPerSample, _lastPhase);
            foreach (var sample in chirp)
            {
                var sin = fileAmplitude * sample;
                var sinBytes = ConvertNumber((long)sin, (byte)_waveFile.BitDepth);
                for (byte channel = 0; channel < (byte)_waveFile.Channels; channel++)
                    _waveFile.AddSampleToEnd(sinBytes);
            }
        }

        private IEnumerable<double> GenerateSineChirp(double frequency1, double frequency2, int length, double xInc, double stratPhase)
        {
            var sineChirp = new double[length];
            var tn = length * xInc;
            this._lastPhase = GetPhase(stratPhase + tn * (frequency1 + (frequency2 - frequency1) / 2));

            for (var i = 0; i < sineChirp.Length; i++)
            {
                var t = i * xInc;
                var delta = t / tn;
                var phase = t * (frequency1 + (frequency2 - frequency1) * delta / 2);
                yield return Math.Sin(phase + stratPhase);
            }
        }

        private double[] GenerateSawtoothWave(double frequency, int length, double xInc, double startPhase, out double endPhase)
        {
            var wave = new double[length];
            var period = Math.PI * 2;
            for (var x = 0; x < length; x++)
            {
                var t = frequency * x * xInc + startPhase;
                wave[x] = 2 * (t / period - Math.Floor(1 / 2 + t / period));
            }
            endPhase = GetPhase(length * xInc * frequency + startPhase);
            return wave;
        }

        private double[] GenerateTriangleWave(double frequency, int length, double xInc, double startPhase, out double endPhase)
        {
            var wave = new double[length];
            var period = Math.PI / 2;
            for (var x = 0; x < length; x++)
            {
                var t = frequency * x * xInc + startPhase;
                wave[x] = 2 / Math.PI * Math.Asin(Math.Sin(Math.PI / 2 / period * t));
            }
            endPhase = GetPhase(length * xInc * frequency + startPhase);
            return wave;
        }

        private double GetPhase(double x)
        {
            double result = 0;
            var sint = Math.Sin(x);
            var cost = Math.Cos(x);
            if (cost > 0 || sint == -1)
                result = Math.Asin(sint);
            else
                result = -Math.Asin(sint) + Math.PI;
            return result;
        }

        private byte[] ConvertNumber(long number, byte bit)
        {
            //It bit depth is 8
            var result = new byte[bit / 8];
            if (bit == 8)
            {
                var signed = Convert.ToSByte(number);
                byte unsigned = 0;
                unsigned = (byte)(128 + signed);
                result[0] = unsigned;
                return result;
            }
            if (bit == 32)
                return BitConverter.GetBytes((int)number);
            if (bit == 16)
                return BitConverter.GetBytes((short)number);
            var fullNumber = BitConverter.GetBytes(number);
            for (var i = 0; i < bit / 8; i++)
            {
                result[i] = fullNumber[i];
            }
            return result;
        }

        public void Save()
        {
            _waveFile.Save();
        }
    }

    public enum BitDepth : byte
    {
        Bit8 = 8,
        Bit16 = 16,
        Bit24 = 24,
        Bit32 = 32
    }
}