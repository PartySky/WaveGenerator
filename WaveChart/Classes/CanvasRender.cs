using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaveChart.Iterfaces;

namespace WaveChart
{
    public class CanvasRender : ICanvasRender
    {
        private INote[] notesData;
        private IWaveReader waveReader;

        public CanvasRender(IWaveReader waveReader)
        {
            this.waveReader = waveReader;
        }

        public void WriteOutPutRender(INote[] notesData)
        {
            var firstNote = waveReader.GetWaveData("filePath");
            throw new NotImplementedException();
        }
    }
}
