using System.Collections.Generic;

public interface ICompressor
{
    double CompressSingleFile(string inputPath, string outputPath);
    double CompressMultipleFiles(string[] inputFiles, string outputPath);
    bool DecompressSingleFile(string compressedPath, string outputPath = null);  // ✅ تم تغيير return type
    void DecompressArchive(string archivePath, string outputFolder = null);
    void ExtractSingleFileFromArchive(string archivePath, string fileNameToExtract, string savePath);
    List<string> GetFileListFromArchive(string archivePath);
    double CompressFolder(string folderPath, string outputPath);
}
