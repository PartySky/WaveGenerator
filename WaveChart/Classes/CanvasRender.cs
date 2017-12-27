using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WaveChart.Iterfaces;

namespace WaveChart
{
    public class CanvasRender : ICanvasRender
    {
        private INote[] notesData;
        private IWaveReader waveReader;
        // Header, Format, Data chunks
        WaveHeader header;
        WaveFormatChunk format;
        WaveDataChunk canvasData;

        public CanvasRender(IWaveReader waveReader)
        {
			this.waveReader = waveReader;
            // Init chunks
            header = new WaveHeader();
            format = new WaveFormatChunk();
            canvasData = new WaveDataChunk();
        }

        public void WriteOutPutRender(INote[] notesData, string filePath)
        {
            var firstNote = waveReader.GetWaveData("filePath");


            // TODO: use readed sample instead of generated sin
            CreateSinForTest();

            // TODO: convert noteStartTime to sampleId
            // and use readed sample data plus noteStartTime
            // like: 
            // canvasData.shortArray[0 + sampleId] = 30000;
            // canvasData.shortArray[2 + sampleId] = 30000;
            // canvasData.shortArray[4 + sampleId] = 30000;
            // canvasData.shortArray[6 + sampleId] = 30000;
            // and so on

            WrieFile(filePath);
        }

        #region test
        public void CreateSinForTest()
        {
            // Number of samples = sample rate * channels * bytes per sample
            uint numSamples = format.dwSamplesPerSec * format.wChannels;

            // Initialize the 16-bit array
            canvasData.shortArray = new short[numSamples];

            int amplitude = 32760;  // Max amplitude for 16-bit audio
            double freq = 440.0f;   // Concert A: 440Hz

            // The "angle" used in the function, adjusted for the number of channels and sample rate.
            // This value is like the period of the wave.
            double t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

            for (uint i = 0; i < numSamples - 1; i++)
            {
                // Fill with a simple sine wave at max amplitude
                for (int channel = 0; channel < format.wChannels; channel++)
                {
                    canvasData.shortArray[i + channel] = Convert.ToInt16(amplitude * Math.Sin(t * i));
                }
            }

            for (uint i = 0; i < numSamples - 1; i++)
            {
                //data.shortArray[i++] = 30000;
                //data.shortArray[i] = 30;
            }

            canvasData.shortArray[0] = 30000;
            canvasData.shortArray[2] = 30000;
            canvasData.shortArray[4] = 30000;
            canvasData.shortArray[6] = 30000;
            canvasData.shortArray[8] = 30000;
            canvasData.shortArray[10] = 30000;
            canvasData.shortArray[12] = 30000;
            canvasData.shortArray[14] = 30000;

            canvasData.shortArray[1] = 30;
            canvasData.shortArray[3] = 30;
            canvasData.shortArray[5] = -3000;
            canvasData.shortArray[7] = -3000;
            canvasData.shortArray[9] = -3000;
            canvasData.shortArray[11] = -30000;
            canvasData.shortArray[13] = -30000;
            canvasData.shortArray[15] = -30000;

            // Calculate data chunk size in bytes

        }
        #endregion

        public uint GetCanvasSize(){
            // TODO: calculate size depended on notes lenght
            return (uint)(canvasData.shortArray.Length * (format.wBitsPerSample / 8));
        }

        public void WrieFile(string filePath)
        {
            canvasData.dwChunkSize = GetCanvasSize();
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File doesn't exist");
                return;
            }
            // Create a file (it always overwrites)
            FileStream fileStream = new FileStream(filePath, FileMode.Create);

            // Use BinaryWriter to write the bytes to the file
            BinaryWriter writer = new BinaryWriter(fileStream);

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
            writer.Write(canvasData.sChunkID.ToCharArray());
            writer.Write(canvasData.dwChunkSize);
            foreach (short dataPoint in canvasData.shortArray)
            {
                writer.Write(dataPoint);
            }

            writer.Seek(4, SeekOrigin.Begin);
            uint filesize = (uint)writer.BaseStream.Length;
            writer.Write(filesize - 8);

            writer.Dispose();
            fileStream.Dispose();
        }
    }
}
