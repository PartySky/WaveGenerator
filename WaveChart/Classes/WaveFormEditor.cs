using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace WaveChart
{
    public static class WaveFormEditor
    {
        public static List<short> AdjustToneDeviations(List<short> sound_data, int targetTone)
        {
            var result = new List<short>();

            var test = false;
            var counter = 0;

            var itemSign = Sign(sound_data.FirstOrDefault());
            var gotFirstPart = false;
            const int wavePeriod = 0; //35;
            var periodThreshold = 0;
            
            foreach (var item in sound_data)
            {
                if (itemSign == Sign(item) * -1)
                {
                    if (counter > periodThreshold + wavePeriod / 2)
                    {
                        if (!gotFirstPart)
                        {
                            result.Add(item);
                            Console.WriteLine(item);
                            
                            gotFirstPart = true;
                            itemSign = itemSign * -1;
                            periodThreshold = counter;
                        }
                        else
                        {
                            break;
                        }
                    }
                    result.Add(item);
                    Console.WriteLine(item);
                }
                else
                {
                    result.Add(item);
                    Console.WriteLine(item);
                }
               counter++;
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
}