using System;
using WaveChart.Iterfaces;

namespace WaveChart
{
    internal class TrackLoader : ITrackLoader
    {
        public INote[] GetTrack(string filePath)
        {
            INote[] notesData = new Note[5];
            return notesData;
        }
    }
}