using System;
using System.IO;
using System.Text;

namespace WaveChart
{
    public class ChartGenerator
    {
        WaveHeader header;
        WaveFormatChunk format;
        WaveDataChunk data;

        public ChartGenerator()
        {
            header = new WaveHeader();
            format = new WaveFormatChunk();
            data = new WaveDataChunk();
        }

        byte[] sRiffType = new byte[40];

        public void Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File doesn't exist, path: {0}", filePath);
                throw new ArgumentNullException(filePath);
                //return;
            }

            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fileStream);

            // Write the header
            //header.sGroupID = reader.ReadString(); // returns more long string then sGroupID 
            header.sGroupID = new string(reader.ReadChars(4));
            //header.dwFileLength = (uint)reader.ReadInt16();
            //header.sRiffType = new string(reader.ReadChars(4));

            // second way
            fileStream.Read(sRiffType, 0, 14);
            Encoding.ASCII.GetString(sRiffType);

            Console.WriteLine("sGroupID {0}", header.sGroupID);
            Console.WriteLine("dwFileLength {0}", header.dwFileLength);
            //Console.WriteLine("sRiffType {0}", header.sRiffType);

            reader.Dispose();
            fileStream.Dispose();
        }
    }
}