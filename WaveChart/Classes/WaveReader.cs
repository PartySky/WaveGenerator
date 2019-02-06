using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaveChart.Iterfaces;

namespace WaveChart
{
    public class WaveReader : IWaveReader
    {
        public short[] GetWaveData(string filePath)
        {
            var waveData = new short[5];
            return waveData;
        }
    }
}
