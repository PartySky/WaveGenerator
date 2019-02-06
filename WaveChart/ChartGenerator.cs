using System;
using System.IO;
using System.Text;

namespace WaveChart
{
    public class ChartGenerator
    {
        WaveHeader header;
        WaveFormatChunk format;
        WaveDataChunk data;

        public ChartGenerator()
        {
            header = new WaveHeader();
            format = new WaveFormatChunk();
            data = new WaveDataChunk();
        }

        byte[] sRiffType = new byte[40];

        public void Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", filePath);
                throw new ArgumentNullException(filePath);
            }

            var fileStream = new FileStream(filePath, FileMode.Open);
            var reader = new BinaryReader(fileStream);

            // Write the header
            //header.sGroupID = reader.ReadString(); // returns more long string then sGroupID 
            header.sGroupID = new string(reader.ReadChars(4));
            header.dwFileLength = (uint)reader.ReadInt16();
            header.sRiffType = new string(reader.ReadChars(4));

            // second way
            fileStream.Read(sRiffType, 0, 14);
            Encoding.ASCII.GetString(sRiffType);

            Console.WriteLine("sGroupID {0}", header.sGroupID);
            Console.WriteLine("dwFileLength {0}", header.dwFileLength);
            Console.WriteLine("sRiffType {0}", header.sRiffType);

            reader.Dispose();
            fileStream.Dispose();
        }

        // Loads a wave/riff audio file.
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
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
                var format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                var format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                var sample_rate = reader.ReadInt32();
                var byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                var data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                var data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
    }
}