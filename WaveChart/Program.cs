using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using static WaveChart.ExceptionHandler;
using static WaveChart.WaveFormEditor;

// test
using System;
using System.Collections.Generic;

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
//            var noteList = GetNoteList(scoresPath);
//            var noteList = GetItemListForTest();
	        var noteList = GetNoteListForTest(player);

            noteList = CalculateSmartRoundRobin(noteList);
            
//            var sound_data = NoteCombinerCombineNotes(noteList);

			var noteListCopy = CreateNoteListCopy(noteList);
	        
	        var sound_data = TestMethod(noteListCopy);

	        var waveFormList = new List<WaveFormToDraw>
	        {
		        new WaveFormToDraw(sound_data, 0)
	        };
			
	        waveFormList.AddRange(GetWaveFormsToDrawFrom(noteList));
	        
	        VideoRender.DrawTrianglesForWaveformList(waveFormList);

	        // Playback.PlayFromArray(sound_data, WAV_CHANNELS, WAV_BPS, SAMPLE_RATE);
        }

        private static List<Note> CreateNoteListCopy(List<Note> noteList)
        {
	        var result = new List<Note>();

	        foreach (var item in noteList)
	        {
		        result.Add(new Note(item.Start, item.PeriodList));
	        }

	        return result;
        }

        private static IEnumerable<WaveFormToDraw> GetWaveFormsToDrawFrom(List<Note> noteList)
        {
	        var result = new List<WaveFormToDraw>();

	        foreach (var item in noteList)
	        {
		        result.Add(new WaveFormToDraw(GetShortsFromPeriods(item.PeriodList), item.Start));
	        }

	        return result;
        }

        private static List<Note> CalculateSmartRoundRobin(List<Note> noteList)
        {
            var result = noteList;
            return result;
        }

        private static IReadOnlyList<Note_old> GetNoteList(string scorespath)
        {
            var fileListToPlay = new string[2];
            fileListToPlay[0] = "Band.wav";
            fileListToPlay[1] = "SidechainLogic.wav";
            
            var noteList = new List<Note_old>{ new Note_old(fileListToPlay[0], 0), new Note_old(fileListToPlay[1], 50)};
            
            return noteList;
        }

        private static List<short> NoteCombinerCombineNotes(IReadOnlyList<Note_old> notes)
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

        private static List<short> GetShortsFromPeriods(List<Period_new> periodUnits)
        {
            var result = new List<short>();

            foreach (var item in periodUnits)
            {
	            result.AddRange(item.ShortsData);
            }

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

        private static List<short> SomeMethod(Playback player, string fileToPlay)
        {
            int channels, bits_per_sample, sample_rate;

            // TODO: figure out if it should be gotten from parameter as sampleList
            var sound_data = player.LoadWave(File.Open(Path.Combine(subFolder, fileToPlay), FileMode.Open),
                // TODO: move it into class like AudioData
                out channels, out bits_per_sample, out sample_rate);

            var sound_data_temp = sound_data.ToList();

            const int tempSoundDataStart = 800000;
            const int tempSoundDataLength = 20;
//            const int tempSoundDataLength = 200000;
            
            sound_data_temp.RemoveRange(0, tempSoundDataStart);
            sound_data_temp
                .RemoveRange(tempSoundDataLength, sound_data_temp.Count - tempSoundDataLength);
            
            var sound_data_mono = WaveFormEditor.GetStereoFromOneChanel(sound_data_temp, 1); 
            var fixed_sound_data = WaveFormEditor.AdjustToneDeviations(sound_data_mono, 440);
            
//            if (true)
//            {
//                Playback.PlayFromArray(fixed_sound_data, 2, 16, 44100);
//            }
//	        return fixed_sound_data;
	        return sound_data_mono;
        }

        public static List<short> TestMethod(List<Note> ItemList)
        {
	        var result = DoTestMethod(ItemList, false);
	        return result;
        }
        
	    public static List<short> TestMethod(List<Note> ItemList, bool draw)
	    {
	        var result = DoTestMethod(ItemList, draw);
	        return result;
	    }

	    private static List<short> DoTestMethod(List<Note> ItemList, bool draw)
	{
		var result = new List<short>();
		
		var counter = 0;
		Note nextNote = null;
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
					nextNote = ItemList[counter + 1];
					isCase_01 = GetItemEnd(item) > nextNote.Start && item.Start < nextNote.Start;
					isCase_02 = GetItemEnd(item) < nextNote.Start && item.Start < nextNote.Start;
				}
				
				if (isCase_01 && !isLastItem)
				{
					Console.WriteLine("Case 1");
					
					var currentItemBlendedPeriodIndex = 0;
					
					var localPeriodIndex = 0;
					var currentPeriodStart = item.Start;
					result.AddRange(GetEmptyShorts(item.Start - positionHead));
					foreach (var period in item.PeriodList)
					{
						if(currentPeriodStart + period.ShortsData.Count < nextNote.Start)
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
							nextNote.Start = currentPeriodStart;
							
							WriteInputDate(ItemList);

							var periodForMultiplyCount = item.PeriodList.Count - localPeriodIndex;
							
							if (periodForMultiplyCount <= nextNote.PeriodList.Count)
							{
								var nextPeriodStartTemp = 0;
								
								for(var i = 0; i < periodForMultiplyCount; i++ )
								{	
									var temp = AdjustPeriodLength(
										item.PeriodList[currentItemBlendedPeriodIndex].ShortsData,
										nextNote.PeriodList[i].ShortsData);

									var blendedShorts = BlendShorts(
										item.PeriodList[currentItemBlendedPeriodIndex].ShortsData, temp);
									
									result.AddRange(blendedShorts);
									if (currentItemBlendedPeriodIndex < item.PeriodList.Count)
									{
										nextPeriodStartTemp = currentPeriodStart +
										                      item.PeriodList[currentItemBlendedPeriodIndex].ShortsData.Count;
										currentPeriodStart = nextPeriodStartTemp;
										currentItemBlendedPeriodIndex++;
									}
								}
								// Cut of multiplied periods from nextItem, because they were added to result
								nextNote.PeriodList = nextNote.PeriodList.GetRange(periodForMultiplyCount, 
									nextNote.PeriodList.Count - periodForMultiplyCount);
								nextNote.Start = nextPeriodStartTemp;

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
					foreach (var period in item.PeriodList)
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

//		foreach (var x in result)
//		{
//			Console.Write(x);
//		}
		
		if (draw)
		{
			VideoRender.DrawTriangles_old(result);
		}

		return result;
	}

	    private static List<Note> GetNoteListForTest(Playback player)
	    {
		    var result = new List<Note>();
		    
		    int channels, bits_per_sample, sample_rate;
		    
		    var sound_data_1 = player.LoadWave(File.Open(Path.Combine(subFolder, "440_test.wav"), FileMode.Open),
			    out channels, out bits_per_sample, out sample_rate).ToList();
		    
		    var sound_data_2 = player.LoadWave(File.Open(Path.Combine(subFolder, "880_test.wav"), FileMode.Open),
			    out channels, out bits_per_sample, out sample_rate).ToList();

		    result.Add(new Note(0, GetPeriodsNewFromShorts(sound_data_1)));
		    result.Add(new Note(500, GetPeriodsNewFromShorts(sound_data_2)));
		    
		    return result;
	    }

	    private static List<Period_new> GetPeriodsNewFromShorts(List<short> soundData)
	    {
		    var result = new List<Period_new>();
		    var periodList = GetPeriodList(soundData);
		    foreach (var item in periodList)
		    {
			    result.Add(new Period_new(soundData
				    .GetRange(item.Start,item.End - item.Start)));
		    }

		    return result;
	    }

	    private static List<Note> GetItemListForTest()
        {
	        var result = new List<Note>(); 
	        var currentCase = 1;
		
	        var period_1 = new List<Period_new>();
	        var period_2 = new List<Period_new>();
	        var period_3 = new List<Period_new>();

	        if(currentCase == 1)
	        {
		        period_1 = new List<Period_new>
		        {
			        new Period_new(new List<short>{1000,1000,1000}),
			        new Period_new(new List<short>{2000,2000,2000}),
			        new Period_new(new List<short>{100,200,300})
		        };
		        period_2 = new List<Period_new>
		        {
			        new Period_new(new List<short>{1000,1000,1000,1000,1000}),
			        new Period_new(new List<short>{2000,2000,2000,2000,2000}),
			        new Period_new(new List<short>{3000,3000,3000,3000,3000}),
			        new Period_new(new List<short>{4000,4000,4000,4000,4000}),
			        new Period_new(new List<short>{5000,5000,5000,5000,5000}),
			        new Period_new(new List<short>{6000,6000,6000,6000,6000})
		        };
		        period_3 = new List<Period_new>
		        {
//				new Period_new(new List<short>{1,1}),
//				new Period_new(new List<short>{2,2}),
//				new Period_new(new List<short>{3,3}),
//				new Period_new(new List<short>{4,4}),
//				new Period_new(new List<short>{5,5}),
//				new Period_new(new List<short>{6,6})

			        new Period_new(new List<short>{1000,1000,1000,1000,1000,1000,1000}),
			        new Period_new(new List<short>{2000,2000,2000,2000,2000,2000,2000}),
			        new Period_new(new List<short>{3000,3000,3000,3000,3000,3000,3000}),
			        new Period_new(new List<short>{4000,4000,4000,4000,4000,4000,4000})
//				new Period_new(new List<short>{5,5,5,5,5,5,5}),
//				new Period_new(new List<short>{6,6,6,6,6,6,6})
		        };
			
		        result = new List<Note>
		        {
			        new Note(0, period_1),
			        new Note(6, period_2),
			        new Note(27, period_3)
		        };
	        }
	        else
	        {
		        period_1 = new List<Period_new>
		        {
			        new Period_new(new List<short>{1000,1000,1000}),
			        new Period_new(new List<short>{1000,1000,1000}),
			        new Period_new(new List<short>{1000,1000,1000})
		        };
		        period_2 = new List<Period_new>
		        {
//					new Period_new(new List<short>{2,2,2,2,2}),
//					new Period_new(new List<short>{2,2,2,2,2}),
//					new Period_new(new List<short>{2,2,2,2,2}),
			        new Period_new(new List<short>{2000,2000,2000,2000,2000})
		        };
			
		        result = new List<Note>{ new Note(2, period_1), new Note(13, period_2)};
	        }

	        return result;
        }


        private static void WriteInputDate(List<Note> itemList)
        {
	        return;
//	        foreach (var item in itemList)
//	        {
//		        Console.WriteLine("");
//
//		        for (int i = 0; i < item.Start; i++)
//		        {
//			        Console.Write(0);
//		        }
//
//		        foreach (var period in item.PeriodList)
//		        {
//			        foreach (var shortTemp in period.ShortsData)
//			        {
//				        Console.Write(shortTemp);
//			        }
//		        }
//	        }
//	        Console.WriteLine("");
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
		
		if(period_1.Count == period_2.Count)
		{
			result = period_2;
		}
		else if(period_1.Count < period_2.Count)
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

			while (result.Count < period_1.Count)
			{
				result.Add(0);
				counter++;
			}
		}

//		var resultTemp = new List<short>();
		
//		foreach (var item in result)
//		{
//			resultTemp.Add((short) (item * 20000));	
//		}
		
		return result;
	}
	
	public static int GetItemEnd(Note note)
	{
		var result = note.Start;
		
		foreach (var period in note.PeriodList)
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

    public class Note_old
    {
        public Note_old(string audio, int? offset)
        {
            AudioFile = audio;
            Start = offset ?? 0;
        }

        public Note_old(string audio)
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
    
    public class Note
    {
	    public Note(int start, List<Period_new> periodList)
	    {
		    Start = start;
		    PeriodList = periodList;
	    }

	    public int Start { get; set; }
	    public List<Period_new> PeriodList { get; set;}
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
