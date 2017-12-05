using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace WaveChart
{
    public class Program
    {
        private static byte[] myWaveData;

        // Sample rate (Or number of samples in one second)
        private const int SAMPLE_FREQUENCY = 44100;
        // 60 seconds or 1 minute of audio
        private const int AUDIO_LENGTH_IN_SECONDS = 1;
		private static char sGroupIDChar1;
		private static char sGroupIDChar2;

		public static byte[] TestWaveData { get; private set; }

		public static void Main(string[] args)
        {	
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            int generationMethod = 2;
			string filePath = @".\ForTests\test2.wav";
			string filePathRead = @".\ForTests\test-read2.wav";
			//int[] test;
			//test = new int[4];
			char[] sGroupIDReaded = new char[4];

			switch (generationMethod)
            {
                case 1:
                    
                    List<Byte> tempBytes = new List<byte>();

                    WaveHeaderA header = new WaveHeaderA();
                    FormatChunk format = new FormatChunk();
                    DataChunk data = new DataChunk();

                    // Create 1 second of tone at 697Hz
					SineGenerator leftData = new SineGenerator(697.0f, SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS);
                    // Create 1 second of tone at 1209Hz
                    SineGenerator rightData = new SineGenerator(1209.0f,
                       SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS);

                    data.AddSampleData(leftData.Data, rightData.Data);

                    header.FileLength += format.Length() + data.Length();

                    tempBytes.AddRange(header.GetBytes());
                    tempBytes.AddRange(format.GetBytes());
                    tempBytes.AddRange(data.GetBytes());

                    myWaveData = tempBytes.ToArray();
					
					filePath = @".\ForTests\test2.wav";

					if (!File.Exists(filePath))
					{
						Console.WriteLine("File doesn't exist");
						break;
					}
					File.WriteAllBytes(filePath, myWaveData);
                    File.WriteAllBytes("./ForTests/data.tsv", myWaveData);
                    break;

                case 2:

					filePath = @".\ForTests\test2.wav";
					//WaveGenerator wave = new WaveGenerator(WaveExampleType.ExampleSineWave);
					WaveGenerator wave = new WaveGenerator(WaveChart.WaveExampleType.ExampleSineWave);
					wave.Save(filePath);
					if (!File.Exists(@".\ForTests\test-read2.wav"))
					{
						Console.WriteLine("File doesn't exist");
						break;
					}
					// Open a file (it always overwrites)
					FileStream fileStream = new FileStream(@".\ForTests\read3.wav", FileMode.Open);

					// Use BinaryReader to read the bytes to the file
					BinaryReader reader = new BinaryReader(fileStream);

					//TestWaveData = File.ReadAllBytes(@".\ForTests\test3.wav");

					//wave.Read(filePathRead);

					//sGroupIDChar1 = reader.ReadChar();
					//sGroupIDChar2 = reader.ReadChar();
					
					for (int i=0; i<4; i++) {
						Console.WriteLine(sGroupIDReaded[i]);
						sGroupIDReaded[i] = reader.ReadChar();
					}

					for (int i=0; i<4; i++)
					{
						Console.WriteLine(sGroupIDReaded[i]);
					}

					//shortArrayReaded

					reader.Dispose();
					fileStream.Dispose();
					break;
                default:
                    break;
            }


            Console.WriteLine();

            host.Run();
        }
    }
}
