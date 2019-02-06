using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace WaveChart
{
    public static class WaveFormEditor
    {
        public static List<short> AdjustToneDeviations(List<short> soundData, int targetTone)
        {
            var result = new List<short>();
            try
            {
                soundData = soundData.GetRange(0, 400);
                
                var test = false;
                var counter = 0;
    
                var itemSign = Sign(soundData.FirstOrDefault());
                var gotFirstPart = false;
                const int wavePeriod = 0; //35;
                var periodThreshold = 0;
                var periodStartTemp = 0;
                
                var periodList = new List<Period>();
                
                foreach (var item in soundData)
                {
                    if (itemSign == Sign(item) * -1)
                    {
                        if (counter > periodThreshold + wavePeriod / 2)
                        {
                            if (!gotFirstPart)
                            {
                                gotFirstPart = true;
                                itemSign = itemSign * -1;
                                periodThreshold = counter;
                            }
                            else
                            {
                                periodList.Add(new Period(periodStartTemp, counter));
                                itemSign = itemSign * -1;
                                gotFirstPart = false;
                                periodStartTemp = counter + 1;
                            }
                        }
                    }
                    counter++;
                }
    
                foreach (var item in periodList)
                {
                    result.AddRange(StratchPeriod(soundData, item));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
           return result;
        }

        private static IEnumerable<short> StratchPeriod(List<short> soundData, Period period)
        {
            var resultTemp = soundData.GetRange(period.Start, period.End - period.Start);
            var result = new List<short>();

            foreach (var item in resultTemp)
            {
                result.Add(item);
                result.Add(item);
                result.Add(item);
                result.Add(item);
                result.Add(item);
                result.Add(item);
            }
            
            return result;
        }


        public static List<short> GetStereoFromOneChanel(IEnumerable<short> sound_data, int channel)
        {
            var result = sound_data
                .Where((n, i) => i % 2 == channel)
                .SelectMany(n => new [] { n, n }).ToList(); 
            
            return result;
        }
    }
    
    public struct Period
    {
        public Period(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int End { get; set; }

        public int Start { get; set; }
    }
}