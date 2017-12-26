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
        private const string fileToWrite = "test2.wav";
        private const string fileToPlay = "test2.wav";
        private const string fileToRead = "test-read2.wav";
        private const string subFolder = "ForTests";

        public static void Main(string[] args)
        {
            bool runHost = false;

            #region runHost
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
            #endregion
            
			string filePathWrite = Path.Combine(
				Directory.GetCurrentDirectory(), subFolder, fileToWrite
			);
            string filePathRead = Path.Combine(
                Directory.GetCurrentDirectory(), subFolder, fileToRead
            );

			WaveGenerator wave = new WaveGenerator(WaveChart.WaveExampleType.ExampleSineWave);
			wave.Save(filePathWrite);
			ChartGenerator chartGenerator = new ChartGenerator();

            Playback player = new Playback();
            player.play(subFolder, fileToPlay);
        }
    }
}
