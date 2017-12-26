namespace WaveChart.Iterfaces
{
    public interface ITrackLoader
    {
        INote[] GetTrack(string filePath);
    }
}
