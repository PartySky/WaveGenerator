public void Save(string filePath)
 {
    // Create a file (it always overwrites)
     FileStream fileStream = new FileStream(filePath, FileMode.Create);   

    // Use BinaryWriter to write the bytes to the file
     BinaryWriter writer = new BinaryWriter(fileStream);
 
    // Write the header
    writer.Write(header.sGroupID.ToCharArray());
     writer.Write(header.dwFileLength);
     writer.Write(header.sRiffType.ToCharArray());
  
     // Write the format chunk
     writer.Write(format.sChunkID.ToCharArray());
     writer.Write(format.dwChunkSize);
     writer.Write(format.wFormatTag);
     writer.Write(format.wChannels);
     writer.Write(format.dwSamplesPerSec);
     writer.Write(format.dwAvgBytesPerSec);
     writer.Write(format.wBlockAlign);
     writer.Write(format.wBitsPerSample);
  
     // Write the data chunk
     writer.Write(data.sChunkID.ToCharArray());
     writer.Write(data.dwChunkSize);
     foreach (short dataPoint in data.shortArray)
     {
         writer.Write(dataPoint);
     }
  
     writer.Seek(4, SeekOrigin.Begin);
     uint filesize = (uint)writer.BaseStream.Length;
     writer.Write(filesize - 8);
     
     // Clean up
     writer.Close();
     fileStream.Close();            
 }