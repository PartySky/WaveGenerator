using Microsoft.VisualBasic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using static WaveChart.ExceptionHandler;

// test
using System.Threading;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using WaveChart.Iterfaces;

using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using static System.Math;

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
        private const string scoresPath = "scoresPath";
        private const int WAV_CHANNELS = 2;
        private const int WAV_BPS = 16;
        private const int SAMPLE_RATE = 44100;

        public static void Main(string[] args)
        {
            TestMethod();
            
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

            var player = new Playback();
            var noteList = GetNoteList(scoresPath);
            noteList = GetNoteListTest();

            noteList = CalculateSmartRoundRobin(noteList);
            
            var sound_data = NoteCombinerCombineNotes(noteList);

            VideoRender.DrawTriangles(sound_data);
            Playback.PlayFromArray(sound_data, WAV_CHANNELS, WAV_BPS, SAMPLE_RATE);
        }

        private static IReadOnlyList<Note> GetNoteListTest()
        {
            var result = new List<Note>();
            return result;
        }

        private static IReadOnlyList<Note> CalculateSmartRoundRobin(IReadOnlyList<Note> noteList)
        {
            var result = noteList;
            return result;
        }

        private static IReadOnlyList<Note> GetNoteList(string scorespath)
        {
            var fileListToPlay = new string[2];
            fileListToPlay[0] = "Band.wav";
            fileListToPlay[1] = "SidechainLogic.wav";
            
            var noteList = new List<Note>{ new Note(fileListToPlay[0], 0), new Note(fileListToPlay[1], 50)};
            
            return noteList;
        }

        private static List<short> NoteCombinerCombineNotes(IReadOnlyList<Note> notes)
        {
            notes = notes.OrderBy(p => p.Start).ToList();
            var periodUnitsTemp = new List<PeriodUnit>();
                
            var counter = 0;

            foreach (var noteItem in notes)
            {
                var nextNote = notes[counter + 1];

                var noteItemEnd = noteItem.Start + noteItem.Length;
                var nextNoteEnd = nextNote.Start + nextNote.Length;

                var isCrossfade = noteItemEnd > nextNote.Start && nextNoteEnd > noteItemEnd;

                if (isCrossfade)
                {
	                var firstPeriodToCFIndex = 0;//FirstCFPeriodIndex(nextNote.Start - noteItem.Start);
                    var periodsBeforeCF = noteItem.PeriodList.GetRange(0, firstPeriodToCFIndex);

                    var numPeriodsToCrossfade = noteItem.PeriodList.Count - firstPeriodToCFIndex;
                    
                    var crossfadedData = GetCrossfadedData(
                        noteItem.PeriodList
                            .GetRange(firstPeriodToCFIndex, numPeriodsToCrossfade),
                        nextNote.PeriodList
                            .GetRange(0, numPeriodsToCrossfade)
                    );
                    
                    periodUnitsTemp.AddRange(periodsBeforeCF);
                    periodUnitsTemp.AddRange(crossfadedData);

                    nextNote.PeriodList = nextNote.PeriodList
                            .GetRange(numPeriodsToCrossfade, 
                                nextNote.PeriodList.Count - numPeriodsToCrossfade);

                    nextNote.Start = noteItemEnd;
                }
                else if (noteItemEnd < nextNote.Start)
                {
                    periodUnitsTemp.AddRange(noteItem.PeriodList);
                    var silentUnitBeforeNextNote = new List<PeriodUnit>(nextNote.Start - noteItemEnd);
                    periodUnitsTemp.AddRange(silentUnitBeforeNextNote);
                }
                else
                {
                    Console.WriteLine("Unknown Case");
                }

                counter++;
            }
                
            var result = new List<short>();

            return result;
        }

        private static List<PeriodUnit> GetCrossfadedData(List<PeriodUnit> firstPeriodList, 
                                                     List<PeriodUnit> secondPeriodList)
        {
            var result = new List<PeriodUnit>();

            var volumeDecreaseSteep = 0; // GetVolumeDecreeseStep(firstPeriodList.Count);
            const int volumeMax = 100;
            const int volumeMin = 0;
            var volumeMaxTemp = volumeMax;
            var volumeMinTemp = volumeMin;
            
            var volumeMaxTemp2 = volumeMax - volumeDecreaseSteep;
            var volumeMinTemp2 = volumeMin + volumeDecreaseSteep;

            var volumeDecreaseSteepForShorts = 0;
            var volumeDecreaseIterator = 0;
            
//            foreach (var item in firstPeriodList)
//            {
//                firstPeriodList[volumeDecreaseIterator].AudioData = FadeoutForShorts(
//                    firstPeriodList[volumeDecreaseIterator].AudioData,
////                    100,
////                    75
//                    volumeMaxTemp,
//                    volumeMaxTemp2
//                );
//
//                secondPeriodList[volumeDecreaseIterator].AudioData = FadeinForShorts(
//                    secondPeriodList[secondPeriodList.Count - volumeDecreaseIterator].AudioData,
////                    0,
////                    25
//                    volumeMinTemp2,
//                    volumeMinTemp
//                );
//
//                volumeDecreaseIterator++;
//                
//                volumeMaxTemp = volumeMaxTemp - volumeDecreaseSteep;
//                volumeMaxTemp2 = volumeMaxTemp2 - volumeDecreaseSteep;
//                volumeMinTemp = volumeMinTemp + volumeDecreaseSteep;
//                volumeMinTemp2 = volumeMinTemp2 + volumeDecreaseSteep;
//            }

            var i = 0;
            foreach (var item in firstPeriodList)
            {
                result.Add(BlendPeriods(item, secondPeriodList[i]));
                i++;
            }

            return result;
        }

        private static PeriodUnit BlendPeriods(PeriodUnit item_1, PeriodUnit item_2)
        {
            var audioDataTemp = new List<short>();
            var i = 0;
            foreach (var item in item_1.AudioData)
            {
                audioDataTemp.Add( (short)(item + item_2.AudioData[i]));
                i++;
            }

            var result = new PeriodUnit(audioDataTemp);
            return result;
        }

        private static List<short> GetShortsFromPeriods(List<PeriodUnit> periodUnits)
        {
            var result = new List<short>();

            return result;
        }


        private static IEnumerable<short> GetSilence(int i)
        {
            var result = new List<short>();
            if (i < 0)
            {
                return result;
            }

            for (int j = 0; j < i; j++)
            {
                result.Add(0);
                result.Add(0);
            }
            return result;
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
            const int tempSoundDataLength = 70000;
//            const int tempSoundDataLength = 200000;
            
            sound_data_temp.RemoveRange(0, tempSoundDataStart);
            sound_data_temp
                .RemoveRange(tempSoundDataLength, sound_data_temp.Count - tempSoundDataLength);
            
            var sound_data_mono = WaveFormEditor.GetStereoFromOneChanel(sound_data_temp, 1); 
            var fixed_sound_data = WaveFormEditor.AdjustToneDeviations(sound_data_mono, 440);
            
            if (true)
            {
                Playback.PlayFromArray(fixed_sound_data, 2, 16, 44100);
            }
        }
        
        
        
        
        
        /// test methods
    public static void TestMethod()
	{
		var result = new List<short>();
	
		var currentCase = 1;
		
		var period_1 = new List<Period_new>();
		var period_2 = new List<Period_new>();
		var period_3 = new List<Period_new>();

		var ItemList = new List<Item>();
		
		if(currentCase == 1)
		{
			period_1 = new List<Period_new>
			{
				new Period_new(new List<short>{1,1,1}),
				new Period_new(new List<short>{2,2,2}),
				new Period_new(new List<short>{3,3,3})
			};
			period_2 = new List<Period_new>
			{
				new Period_new(new List<short>{1,1,1,1,1}),
				new Period_new(new List<short>{2,2,2,2,2}),
				new Period_new(new List<short>{3,3,3,3,3}),
				new Period_new(new List<short>{4,4,4,4,4}),
				new Period_new(new List<short>{5,5,5,5,5}),
				new Period_new(new List<short>{6,6,6,6,6})
			};
			period_3 = new List<Period_new>
			{
//				new Period_new(new List<short>{1,1}),
//				new Period_new(new List<short>{2,2}),
//				new Period_new(new List<short>{3,3}),
//				new Period_new(new List<short>{4,4}),
//				new Period_new(new List<short>{5,5}),
//				new Period_new(new List<short>{6,6})

				new Period_new(new List<short>{1,1,1,1,1,1,1}),
				new Period_new(new List<short>{2,2,2,2,2,2,2}),
				new Period_new(new List<short>{3,3,3,3,3,3,3}),
				new Period_new(new List<short>{4,4,4,4,4,4,4}),
				new Period_new(new List<short>{5,5,5,5,5,5,5}),
				new Period_new(new List<short>{6,6,6,6,6,6,6})
			};
			
			ItemList = new List<Item>
			{
				new Item(2, period_1), 
				new Item(6, period_2)
//				new Item(27, period_3)
			};
		}
		else
		{
			period_1 = new List<Period_new>
			{
				new Period_new(new List<short>{1,1,1}),
				new Period_new(new List<short>{1,1,1}),
				new Period_new(new List<short>{1,1,1})
			};
			period_2 = new List<Period_new>
			{
//					new Period_new(new List<short>{2,2,2,2,2}),
//					new Period_new(new List<short>{2,2,2,2,2}),
//					new Period_new(new List<short>{2,2,2,2,2}),
				new Period_new(new List<short>{2,2,2,2,2})
			};
			
			ItemList = new List<Item>{ new Item(2, period_1), new Item(13, period_2)};
		}
		
		// Case 1
		
		// 1) Start data:
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _ _'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'	
		//
		// 2) Check if [N - 1]th and [N]th has multiplyed periods. They have it.
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _ _'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'
		//
		// 3) Make the start of nearest period of [N - 1]th item as the start of [N]th item. 
		//    The start of nearest is 5.
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'
		//
		// 4) Get count of multiplyed periods of [N - 1]th item. MultiplyedPeriods = 2. They leghts are 3 and 3.
		//    Apply length for MultiplyedPeriods == 2 of [N]th item
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _'2 2 2'2 2 2'2 2 2 2 2'2 2 2 2 2'
		//
		// Console
		// .Write: _ _ 1 1 1 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2
		
		
		// Case 2
		
		// 1) Start data:
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _ _ _ _ _ _ _ _ _'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'	
		//
		// 2) Check if [N - 1]th and [N]th has multiplyed periods. They have no it.
		//
		//  [0]th:    '1 1 1'1 1 1'1 1 1'
		//  [1]th: _ _ _ _ _ _ _ _ _ _ _ _ _'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'2 2 2 2 2'
		
		//
		// Console
		// .Write: _ _ 1 1 1 1 1 1 1 1 1 _ _ 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2 2
		
		
		var counter = 0;
		Item nextItem = null;
		var isCase_01 = false;
		var isCase_02 = false;
		var positionHead = 0;
		try
		{
			while (ItemList.Count > counter)
			{
				WriteInputDate(ItemList);

				var item = ItemList[counter];
				var isLastItem = counter + 1 == ItemList.Count;
				
				if (!isLastItem)
				{
					nextItem = ItemList[counter + 1];
					isCase_01 = GetItemEnd(item) > nextItem.Start && item.Start < nextItem.Start;
					isCase_02 = GetItemEnd(item) < nextItem.Start && item.Start < nextItem.Start;
				}
				
				if (isCase_01 && !isLastItem)
				{
					Console.WriteLine("Case 1");
					
					var currentItemBlendedPeriodIndex = 0;
					
					var localPeriodIndex = 0;
					var currentPeriodStart = item.Start;
					result.AddRange(GetEmptyShorts(item.Start - positionHead));
					foreach (var period in item.Period)
					{
						if(currentPeriodStart + period.ShortsData.Count < nextItem.Start)
						{
							result.AddRange(period.ShortsData);
							currentPeriodStart = currentPeriodStart + period.ShortsData.Count;
							localPeriodIndex++;
							currentItemBlendedPeriodIndex = localPeriodIndex;
						}
						else
						{
							// Case 1. 3)
							// Shift nextItem.Start to previous item's start
							nextItem.Start = currentPeriodStart;
							
							WriteInputDate(ItemList);

							var periodForMultiplyCount = item.Period.Count - localPeriodIndex;
							
							if (periodForMultiplyCount <= nextItem.Period.Count)
							{
								var nextPeriodStartTemp = 0;
								
								for(var i = 0; i < periodForMultiplyCount; i++ )
								{	
									var temp = AdjustPeriodLength(
										item.Period[currentItemBlendedPeriodIndex].ShortsData,
										nextItem.Period[i].ShortsData);

									var blendedShorts = BlendShorts(
										item.Period[currentItemBlendedPeriodIndex].ShortsData, temp);
									
									result.AddRange(blendedShorts);
									if (currentItemBlendedPeriodIndex < item.Period.Count)
									{
										nextPeriodStartTemp = currentPeriodStart +
										                      item.Period[currentItemBlendedPeriodIndex].ShortsData.Count;
										currentPeriodStart = nextPeriodStartTemp;
										currentItemBlendedPeriodIndex++;
									}
								}
								// Cut of multiplied periods from nextItem, because they were added to result
								nextItem.Period = nextItem.Period.GetRange(periodForMultiplyCount, 
									nextItem.Period.Count - periodForMultiplyCount);
								nextItem.Start = nextPeriodStartTemp;

								break;
							}
							else
							{
								Console.WriteLine("Unknonwn case. Next note has no periods to merge");
							}
						}
					}
				}
				else if (isCase_02 || isLastItem)
				{
					var message = isCase_02 ? "Case 2" : "Last item";
					Console.WriteLine(message);

					Console.WriteLine("Position " + positionHead);
					
					
					
					result.AddRange(GetEmptyShorts(item.Start - positionHead));
					foreach (var period in item.Period)
					{
						result.AddRange(period.ShortsData);
					}
//					positionHead = positionHead + GetItemEnd(item);
				}
				else
				{
					Console.WriteLine("Unknown Case");
				}
				
				positionHead = positionHead + GetItemEnd(item);
				counter++;
			}
		}
		catch (Exception e)
		{
			HandleException(e.Message);
		}

		foreach (var x in result)
		{
			Console.Write(x);
		}
	}

        private static void WriteInputDate(List<Item> itemList)
        {
	        foreach (var item in itemList)
	        {
		        Console.WriteLine("");

		        for (int i = 0; i < item.Start; i++)
		        {
			        Console.Write(0);
		        }

		        foreach (var period in item.Period)
		        {
			        foreach (var shortTemp in period.ShortsData)
			        {
				        Console.Write(shortTemp);
			        }
		        }
	        }
	        Console.WriteLine("");
        }

        private static IEnumerable<short> BlendShorts(IReadOnlyCollection<short> data_1, IReadOnlyList<short> data_2)
        {
	        var result = new List<short>();
	        if (data_1.Count != data_2.Count)
	        {
		        HandleException("Different blended items length");   
	        }

	        var counter = 0;
	        foreach (var item in data_1)
	        {
				result.Add((short) (item + data_2[counter]));
				counter++;
	        }

	        return result;
        }

        public static List<short> AdjustPeriodLength(List<short> period_1, List<short> period_2)
	{
		var result = new List<short>();
		var counter = 0;
		
		if(period_1.Count < period_2.Count)
		{
			foreach (short x in period_1)
			{
				result.Add(period_2[counter]);
				counter++;
			}
		}
		else
		{
			result.AddRange(period_2);
			foreach (short x in period_2)
			{
				result.Add(0);
				counter++;
			}
		}
		return result;
	}
	
	public static int GetItemEnd(Item item)
	{
		var result = item.Start;
		
		foreach (var period in item.Period)
		{
			result = result + period.ShortsData.Count;
		}
		
		return result;
	}
	
	public static List<short> GetEmptyShorts(int num)
	{
		var result = new List<short>();
		try
		{
			var counter = 0;
			while(counter < num)
			{
				result.Add(0);
				counter++;
			}
		}
		catch (Exception e)
		{
			HandleException(e.Message);
		}
		return result;
	}
    }

    public class Note
    {
        public Note(string audio, int? offset)
        {
            AudioFile = audio;
            Start = offset ?? 0;
        }

        public Note(string audio)
        {
            AudioFile = audio;
            Start = 0;
        }

        public string AudioFile { get; set; }
        public List<short> AudioData { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<PeriodUnit> PeriodList { get; set; }
    }

    public class PointsPair
    {
        public PointsPair(int start, int end)
        {
            Start = start;
            End = end;
        }
        public int Start { get; set; }
        public int End { get; set; }

    }

    public class PeriodUnit
    {
        public PeriodUnit(List<short> audioData)
        {
            AudioData = audioData;
        }

        public List<short> AudioData { get; set; }
    }
    
    /// <summary>
    ///  Test
    /// </summary>
    public class Item
    {
	    public Item(int start, List<Period_new> period)
	    {
		    Start = start;
		    Period = period;
	    }

	    public int Start { get; set; }
	    public List<Period_new> Period { get; set;}
    }

    public class Period_new
    {
	    public Period_new(List<short> shortsData)
	    {
		    ShortsData = shortsData;
	    }
	    public List<short> ShortsData { get; set;}
    }
}
