using System;
using System.IO;

namespace WaveChart
{
	public enum WaveExampleType
	{
		ExampleSineWave = 0
	}

    public class WaveGenerator
    {
        // Header, Format, Data chunks
        WaveHeader header;
        WaveFormatChunk format;
        WaveDataChunk data;

		public WaveGenerator(WaveExampleType type)
        {
            // Init chunks
            header = new WaveHeader();
            format = new WaveFormatChunk();
            data = new WaveDataChunk();

            // Fill the data array with sample data
            switch (type)
            {
                case WaveExampleType.ExampleSineWave:

                    // Number of samples = sample rate * channels * bytes per sample
                    var numSamples = format.dwSamplesPerSec * format.wChannels;

                    // Initialize the 16-bit array
                    data.shortArray = new short[numSamples];

                    var amplitude = 32760;  // Max amplitude for 16-bit audio
                    double freq = 440.0f;   // Concert A: 440Hz

                    // The "angle" used in the function, adjusted for the number of channels and sample rate.
                    // This value is like the period of the wave.
                    var t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

                    for (uint i = 0; i < numSamples - 1; i++)
                    {
                        // Fill with a simple sine wave at max amplitude
                        for (var channel = 0; channel < format.wChannels; channel++)
                        {
                            data.shortArray[i + channel] = Convert.ToInt16(amplitude * Math.Sin(t * i));
                        }
                    }

                    for (uint i = 0; i < numSamples - 1; i++)
                    { 
                        //data.shortArray[i++] = 30000;
						//data.shortArray[i] = 30;
                    }

                    data.shortArray[0] = 30000;
                    data.shortArray[2] = 30000;
                    data.shortArray[4] = 30000;
                    data.shortArray[6] = 30000;
                    data.shortArray[8] = 30000;
                    data.shortArray[10] = 30000;
                    data.shortArray[12] = 30000;
                    data.shortArray[14] = 30000;

                    data.shortArray[1] = 30;
                    data.shortArray[3] = 30;
                    data.shortArray[5] = 30;
                    data.shortArray[7] = 30;
                    data.shortArray[9] = 30;
                    data.shortArray[11] = -30000;
                    data.shortArray[13] = -30000;
                    data.shortArray[15] = -30000;

					// Calculate data chunk size in bytes
					data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

                    break;
            }
        }

        public void Save(string filePath)
        {
			if (!File.Exists(filePath)) {
				Console.WriteLine("File doesn't exist");
				return;
			}
            // Create a file (it always overwrites)
            var fileStream = new FileStream(filePath, FileMode.Create);

            // Use BinaryWriter to write the bytes to the file
            var writer = new BinaryWriter(fileStream);

            // Write the header
            writer.Write(header.sGroupID.ToCharArray());
            writer.Write(header.dwFileLength);
            writer.Write(header.sRiffType.ToCharArray());

            // Write the format chunk
            writer.Write(format.sChunkID.ToCharArray());
            writer.Write(format.dwChunkSize);
            writer.Write(format.wFormatTag);
            writer.Write(format.wChannels);
            writer.Write(format.dwSamplesPerSec);
            writer.Write(format.dwAvgBytesPerSec);
            writer.Write(format.wBlockAlign);
            writer.Write(format.wBitsPerSample);

            // Write the data chunk
            writer.Write(data.sChunkID.ToCharArray());
            writer.Write(data.dwChunkSize);
            foreach (var dataPoint in data.shortArray)
            {
                writer.Write(dataPoint);
            }

            writer.Seek(4, SeekOrigin.Begin);
            var filesize = (uint)writer.BaseStream.Length;
            writer.Write(filesize - 8);

			writer.Dispose();
            fileStream.Dispose();
        }
	}
}