namespace WaveChart.Iterfaces
{
    public interface ITrackLoader
    {
        INoteOld[] GetTrack(string filePath);
    }
}
