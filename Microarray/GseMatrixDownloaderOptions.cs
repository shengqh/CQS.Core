using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Microarray
{
  public class GseMatrixDownloaderOptions : AbstractOptions
  {
    public GseMatrixDownloaderOptions()
    { }

    [Option('i', "inputDirectory", Required = true, MetaValue = "DIRECTORY", HelpText = "Input directory which contains GSE directories")]
    public string InputDirectory { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDirectory))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDirectory));
      }

      if (GseDirectories().Length == 0)
      {
        ParsingErrors.Add(string.Format("No GSE directory found in {0}.", this.InputDirectory));
      }

      return ParsingErrors.Count == 0;
    }

    public string[] GseDirectories()
    {
      if (Path.GetFileName(this.InputDirectory).ToUpper().StartsWith("GSE"))
      {
        return new[] { this.InputDirectory };
      }
      else
      {
        return Directory.GetDirectories(this.InputDirectory, "GSE*");
      }
    }
  }
}
