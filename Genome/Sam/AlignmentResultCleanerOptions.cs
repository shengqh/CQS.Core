using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Sam
{
  public class AlignmentResultCleanerOptions : AbstractOptions
  {
    public AlignmentResultCleanerOptions()
    {
      DeletionMode = false;
    }
    [Option('i', "rootDirectory", Required = true, MetaValue = "DIRECTORY", HelpText = "Directory need to be cleaned")]
    public string RootDirectory { get; set; }

    [Option('d', "delete", DefaultValue = false, HelpText = "Deletion mode (default is view mode)")]
    public bool DeletionMode { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(RootDirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", RootDirectory));
        return false;
      }

      return true;
    }
  }
}