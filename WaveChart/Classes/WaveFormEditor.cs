using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Tree;
using static System.Math;
using static WaveChart.ExceptionHandler;

namespace WaveChart
{
    public static class WaveFormEditor
    {
        private const int CROSSFADE_TYPE_FADEIN = 0;
        private const int CROSSFADE_TYPE_FADEOUT = 1;
        
        public static List<Period> GetPeriodList(List<short> soundData)
        {
            var result = new List<Period>();
            try
            {
//                soundData = soundData.GetRange(0, 400);
                
                var counter = 0;
    
                var itemSign = Sign(soundData.FirstOrDefault());
                var gotFirstPart = false;
                const int wavePeriod = 0; //35;
                var periodThreshold = 0;
                var periodStartTemp = 0;
                
                if(itemSign == 0)
                {
                    itemSign = Sign(soundData.Skip(counter).FirstOrDefault(p => p != 0)) * -1;
                    gotFirstPart = true;
                }
                
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
                                result.Add(new Period(periodStartTemp, counter));
                                itemSign = itemSign * -1;
                                gotFirstPart = false;
                                periodStartTemp = counter + 1;
                            }
                        }
                    }
                    counter++;
                }
            }
            catch (Exception e)
            {
                HandleException(e.Message);
            }
            return result;
        }
        
        public static List<short> AdjustToneDeviations(List<short> soundData, int targetTone)
        {
            var result = new List<short>();
            try
            {   
                var periodList = GetPeriodList(soundData);
    
                foreach (var item in periodList)
                {
                    result.AddRange(StratchPeriod(soundData, item, targetTone));
                }
            }
            catch (Exception e)
            {
                HandleException(e.Message);
            }
           return result;
        }

        private static IEnumerable<short> StratchPeriod(List<short> soundData, Period period, int targetTone)
        {
            var targetItemsCount = GetTargetItemsCount(targetTone);
            
            var resultTemp = soundData.GetRange(period.Start, period.End - period.Start);

            const bool test = true;
            if (test)
            {
                targetItemsCount = resultTemp.Count * 130 / 100;
            }

            var increment = Convert.ToDouble(resultTemp.Count) / targetItemsCount;

            var result = Enumerable.Range(0, targetItemsCount).
                Select(x => resultTemp[(int)(x * increment)]).
                ToList();
            
            return result;
        }

        private static int GetTargetItemsCount(int targetTone)
        {
//            return 5;
            return 400;
        }

        public static List<short> GetStereoFromOneChanel(IEnumerable<short> sound_data, int channel)
        {
            var result = sound_data
                .Where((n, i) => i % 2 == channel)
                .SelectMany(n => new [] { n, n }).ToList(); 
            
            return result;
        }
    
        public static List<short> MakeFadeOut(IEnumerable<short> sound_data, int start)
        {
            var result = MakeCrossFade(sound_data, start, 0, CROSSFADE_TYPE_FADEOUT);
            
            return result;
        }
        
        public static List<short> MakeFadeIn(IEnumerable<short> sound_data, int end)
        {
            var result = MakeCrossFade(sound_data, 0, end, CROSSFADE_TYPE_FADEIN);
            
            return result;
        }
        
        private static List<short> MakeCrossFade(IEnumerable<short> sound_data, int start, int end, int type)
        {
            var result = new List<short>();
            var fadedItemsCount = GetFadedItemsCount(sound_data.Count(),start, end, type);
            
            switch (type)
            {
                case CROSSFADE_TYPE_FADEIN:
                {
                    foreach (var item in sound_data)
                    {
                        
                    }
                }
                    break;
                case CROSSFADE_TYPE_FADEOUT:
                    break;
            }

            return result;
        }

        private static int GetFadedItemsCount(int audioLength, int start, int end, int type)
        {
            var result = 0;
            switch (type)
            {
                case CROSSFADE_TYPE_FADEIN:
                {
                    result = audioLength - end;
                    break;
                }
                case CROSSFADE_TYPE_FADEOUT:
                {
                    result = audioLength - start;
                    break;
                }
            }
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