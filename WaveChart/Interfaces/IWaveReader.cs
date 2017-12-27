namespace WaveChart.Iterfaces
{
    public interface IWaveReader
    {
        short[] GetWaveData(string filePath);
    }
}
