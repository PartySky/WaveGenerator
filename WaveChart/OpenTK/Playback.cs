using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Linq;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using static System.BitConverter;

// TODO: rename it
namespace WaveChart
{
    public class Playback
    {
        // TODO: rename to blend multiply files, remove playing from the method
        public void playMultiplyFiles(string subFolder, string fileToPlay, string[] fileListToPlay)
        {
            using (var context = new AudioContext())
            {
                int channels, bits_per_sample, sample_rate;

                var sound_data1 = LoadWave(File.Open(Path.Combine(subFolder, fileListToPlay[0]), FileMode.Open),
                                             out channels, out bits_per_sample, out sample_rate);

                var sound_data2 = LoadWave(File.Open(Path.Combine(subFolder, fileListToPlay[1]), FileMode.Open),
                                             out channels, out bits_per_sample, out sample_rate);
                
                var result_sound_data_short = sound_data1.Length <= sound_data2.Length ? new short[sound_data1.Length] : new short[sound_data2.Length];
                var z = 0;

                for (var i = 0; i < result_sound_data_short.Length; i++)
                {
                    result_sound_data_short[i] = (short)((sound_data1[i] + sound_data2[i]) / 2);
                }

                PlayFromArray(result_sound_data_short.ToList(), 2, 16, 44100);
            }
        }


        public static void PlayFromArray(List<short> result_sound_data_short, int channels, int bits_per_sample, int sample_rate)
        {
            using (var context = new AudioContext())
            {
                var buffer = AL.GenBuffer();
                var source = AL.GenSource();
                int state;
                
                var result_sound_data = new byte[result_sound_data_short.Count * 2];
    
                var z1 = 0;
    
                foreach (var t in result_sound_data_short)
                {
                    result_sound_data[z1] = GetBytes(t)[0];
                    z1++;
                    result_sound_data[z1] = GetBytes(t)[1];
                    z1++;
                }
                AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), result_sound_data, result_sound_data.Length, sample_rate);
    
                AL.Source(source, ALSourcei.Buffer, buffer);
                AL.SourcePlay(source);
    
                Console.WriteLine("Playing");
                do
                {
                    Thread.Sleep(250);
                    Console.WriteLine(".");
                    AL.GetSource(source, ALGetSourcei.SourceState, out state);
                }
                while ((ALSourceState)state == ALSourceState.Playing);
    
                Console.WriteLine("");
    
                AL.SourceStop(source);
                AL.DeleteSource(source);
                AL.DeleteBuffer(buffer);
            }
        }

        public void play(string subFolder, string fileToPlay){
            using (var context = new AudioContext())
            {
                var buffer = AL.GenBuffer();
                var source = AL.GenSource();
                int state;
                var filePathPlay = Path.Combine(
                    Directory.GetCurrentDirectory(), subFolder, fileToPlay
                );


                int channels, bits_per_sample, sample_rate;
                if (!File.Exists(filePathPlay))
                {
                    Console.WriteLine("File doesn't exist, path: {0}", filePathPlay);
                    throw new ArgumentNullException(filePathPlay);
                }

                // it returns 
                var sound_data = new byte[5];
                //byte[] sound_data = LoadWave(File.Open(filePathPlay, FileMode.Open), 
                                             //out channels, out bits_per_sample, out sample_rate);

                var sound_data_int = new short[sound_data.Length/2];

                var iInt = 0;
                for (var iByte = 0; iByte < sound_data.Length - 1; iByte = iByte + 2)
                {
                    sound_data_int[iInt] = ToInt16(sound_data, iByte);
					iInt++;
                }


                //AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);

                AL.Source(source, ALSourcei.Buffer, buffer);
                AL.SourcePlay(source);

                Console.WriteLine("Playing");
                //Trace.Write("Playing");

                // Query the source to find out when it stops playing.
                do
                {
                    Thread.Sleep(250);
                    Console.WriteLine(".");
                    //Trace.Write(".");
                    AL.GetSource(source, ALGetSourcei.SourceState, out state);
                }
                while ((ALSourceState)state == ALSourceState.Playing);

                Console.WriteLine("");
                //Trace.WriteLine("");

                AL.SourceStop(source);
                AL.DeleteSource(source);
                AL.DeleteBuffer(buffer);
            }
        }

        // Loads a wave/riff audio file.
        //public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        //public static short[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        public short[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var reader = new BinaryReader(stream))
            {
                // RIFF header
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                var riff_chunck_size = reader.ReadInt32();

                var format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                var junk_signature = new string(reader.ReadChars(4));

                // Handlink JUNK header
                if (junk_signature == "JUNK")
                {
                    var chunk_size = reader.ReadInt32();
                    stream.Position = stream.Position + chunk_size;
                } else {
                    // Go back to 4 bytes and let reader read signature again
                    stream.Position = stream.Position - 4;
                }

                var format_signature = new string(reader.ReadChars(4));

                if (format_signature != "fmt ") {
                    throw new NotSupportedException("Format signature is not fmt");
                }

                var format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                var sample_rate = reader.ReadInt32();
                var byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                var data_signature = new string(reader.ReadChars(4));


                if (data_signature != "data" && data_signature != "FLLR") {
                    throw new NotSupportedException("Data signature neither data nor FLLR");
                }

                if (data_signature != "FLLR")
                {
                    // what is it FLLR?
                }

                var data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                var x = new short[((int)reader.BaseStream.Length - reader.BaseStream.Position) / 2];

                var i = 0;
                while (reader.BaseStream.Position < x.Length)
                {
                    x[i++] = reader.ReadInt16();
                }

                return x;
            }
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}
