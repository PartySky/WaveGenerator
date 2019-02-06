using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

// test
using System.Threading;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using WaveChart.Iterfaces;

using System;
using OpenTK.Graphics;

namespace WaveChart
{
    public static class Program
    {
        private static byte[] myWaveData;

        // Sample rate (Or number of samples in one second)
        private const int SAMPLE_FREQUENCY = 44100;
        // 60 seconds or 1 minute of audio
        private const int AUDIO_LENGTH_IN_SECONDS = 1;
		public static byte[] TestWaveData { get; private set; }
        private const string fileToWrite = "output.wav";
        private const string fileToPlay = "Guitar.wav";
        private const string fileToRead = "test-read2.wav";
        private const string subFolder = "ForTests";
        private const string scoresPath = "scoreskPath";

        public static void Main(string[] args)
        {
            var parentFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, subFolder);
            const bool runHost = false;

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

            ITrackLoader trackLoader = new TrackLoader();
            var notesData = trackLoader.GetTrack(scoresPath);
            IWaveReader waveReader = new WaveReader();
            ICanvasRender canvasRender = new CanvasRender(waveReader);

            var filePathWrite = Path.Combine(parentFolder, fileToWrite);
            var filePathRead = Path.Combine(parentFolder, fileToRead);

			//WaveGenerator wave = new WaveGenerator(WaveChart.WaveExampleType.ExampleSineWave);
			//wave.Save(filePathWrite);
			//ChartGenerator chartGenerator = new ChartGenerator();

            var player = new Playback();
            var fileListToPlay = new string[2];
            fileListToPlay[0] = "Band.wav";
            fileListToPlay[1] = "SidechainLogic.wav";

            SomeMethod(player, fileListToPlay[0]);
            
            if (false)
            {
                player.playMultiplyFiles(parentFolder, fileToPlay, fileListToPlay);
            }

            if (false)
            {
                canvasRender.WriteOutPutRender(notesData, filePathWrite);
            }
        }

        private static void SomeMethod(Playback player, string fileToPlay)
        {
            int channels, bits_per_sample, sample_rate;

            // TODO: figure out if it should be gotten from parameter as sampleList
            var sound_data = player.LoadWave(File.Open(Path.Combine(subFolder, fileToPlay), FileMode.Open),
                // TODO: move it into class like AudioData
                out channels, out bits_per_sample, out sample_rate);

            var sound_data_temp = sound_data.ToList();

            const int tempSoundDataStart = 800000;
            const int tempSoundDataLegit = 50;
            
            sound_data_temp.RemoveRange(0, tempSoundDataStart);
//            sound_data_temp.RemoveRange(tempSoundDataLegit, sound_data_temp.Count - tempSoundDataLegit);
            
            var sound_data_mono = WaveFormEditor.GetStereoFromOneChanel(sound_data_temp, 1); 
            var fixed_sound_data = WaveFormEditor.AdjustToneDeviations(sound_data_mono, 440);
            
            VideoRender.DrawTriangles(fixed_sound_data);
            
            // lets test it
            if (true)
            {
                var parentFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, subFolder);

                Playback.PlayFromArray(fixed_sound_data, 2, 16, 44100);
            }
        }
    }
}
