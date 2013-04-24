using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.TCGA
{
  public interface ITCGATechnology
  {
    /// <summary>
    /// Node name from TCGA web site
    /// </summary>
    string NodeName { get; }

    /// <summary>
    /// Get reader for expression data
    /// </summary>
    /// <returns>reader</returns>
    IFileReader<ExpressionData> GetReader();

    /// <summary>
    /// Get count reader for expression data. If there is no count information for current technology, return null.
    /// </summary>
    /// <returns>count reader</returns>
    IFileReader<ExpressionData> GetCountReader();

    /// <summary>
    /// Get reader based on data type. If there is no count information for current technology, always return GetReader result. 
    /// </summary>
    /// <param name="tdt">data type (normalized/count)</param>
    /// <returns></returns>
    IFileReader<ExpressionData> GetReader(TCGADataType tdt);

    /// <summary>
    /// Is current node matches to current technology?
    /// </summary>
    /// <param name="node">node from TCGA web site</param>
    /// <returns>true/false</returns>
    bool IsData(SpiderTreeNode node);

    /// <summary>
    /// Get corresponding filename based on data type.
    /// Sometimes, we need to get count information based on normalized information. Since those two data may be
    /// located at different files, we need to get count file from normalized file.
    /// </summary>
    /// <param name="filename">normalized data file</param>
    /// <returns>count data file</returns>
    string GetCountFilename(string filename);

    /// <summary>
    /// Get finder for parsing filename to participant information
    /// directory: tumorDir/data/technology/platform
    /// </summary>
    /// <param name="tumorDir">tumor directory</param>
    /// <param name="platformDir">platform directory</param>
    /// <returns>finder</returns>
    IParticipantFinder GetFinder(string tumorDir, string platformDir);

    /// <summary>
    /// Get default filename filter
    /// </summary>
    /// <returns>filename filter</returns>
    Func<string, bool> GetFilenameFilter();

    /// <summary>
    /// Get barcode/file map
    /// </summary>
    /// <param name="tumordir">tumor directory</param>
    /// <param name="fileFilter">file filter, if null assigned, the default file filter will be used</param>
    /// <returns>barcode/file map</returns>
    Dictionary<string, List<BarInfo>> GetFiles(string tumordir, Func<string, bool> fileFilter);

    /// <summary>
    /// Get dataset information from tumor directory
    /// </summary>
    /// <param name="tumorDir">tumor directory</param>
    /// <param name="tumorDir">file filter</param>
    /// <returns>dataset information</returns>
    DatasetInfo GetDataset(string tumordir, Func<string, bool> fileFilter);

    /// <summary>
    /// Get technology directory based on tumor directory
    /// </summary>
    /// <param name="tumorDir">tumor directory</param>
    /// <returns>technology directory</returns>
    string GetTechnologyDirectory(string tumorDir);
  }
}
