using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

// test
using System.Threading;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace WaveChart
{
    public class Program
    {
        private static byte[] myWaveData;

        // Sample rate (Or number of samples in one second)
        private const int SAMPLE_FREQUENCY = 44100;
        // 60 seconds or 1 minute of audio
        private const int AUDIO_LENGTH_IN_SECONDS = 1;
		public static byte[] TestWaveData { get; private set; }
        private const int generationMethod = 2;
        private const string fileToWrite = "test2d.wav";
        private const string fileToRead = "test-read2.wav";
        private const string subFolder = "ForTests";

		public static void Main(string[] args)
        {
            bool runHost = false;

            if (runHost) { 
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();
                host.Run();
			};
            
            string filePathWrite = Path.Combine(
                Directory.GetCurrentDirectory(), subFolder, fileToWrite
            );
            string filePathRead = Path.Combine(
                Directory.GetCurrentDirectory(), subFolder, fileToRead
            );

			switch (generationMethod)
            {
                case 1:
                    
                    List<Byte> tempBytes = new List<byte>();

                    WaveHeaderA header = new WaveHeaderA();
                    FormatChunk format = new FormatChunk();
                    DataChunk data = new DataChunk();

                    // Create 1 second of tone at 697Hz
					SineGenerator leftData = new SineGenerator(
                        697.0f, SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS
                    );
                    // Create 1 second of tone at 1209Hz
                    SineGenerator rightData = new SineGenerator(
                        1209.0f,SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS
                    );

                    data.AddSampleData(leftData.Data, rightData.Data);

                    header.FileLength += format.Length() + data.Length();

                    tempBytes.AddRange(header.GetBytes());
                    tempBytes.AddRange(format.GetBytes());
                    tempBytes.AddRange(data.GetBytes());

                    myWaveData = tempBytes.ToArray();
					
					filePathWrite = @".\ForTests\test2.wav";

					if (!File.Exists(filePathWrite))
					{
						Console.WriteLine("File doesn't exist");
						break;
					}
					File.WriteAllBytes(filePathWrite, myWaveData);
                    File.WriteAllBytes("./ForTests/data.tsv", myWaveData);
                    break;

                case 2:
					WaveGenerator wave = new WaveGenerator(WaveChart.WaveExampleType.ExampleSineWave);
                    wave.Save(filePathWrite);
					ChartGenerator chartGenerator = new ChartGenerator();
                    //chartGenerator.Read(filePathRead);
					break;
            }

            Playback player = new Playback();
            player.play(subFolder);
        }
    }
}
