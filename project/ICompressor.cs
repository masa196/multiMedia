using System.Collections.Generic;

public interface ICompressor
{
    double CompressSingleFile(string inputPath, string outputPath);
    double CompressMultipleFiles(string[] inputFiles, string outputPath);
    void DecompressSingleFile(string compressedPath, string outputPath = null);
    void DecompressArchive(string archivePath, string outputFolder = null);
    void ExtractSingleFileFromArchive(string archivePath, string fileNameToExtract, string savePath);
    List<string> GetFileListFromArchive(string archivePath);
}
