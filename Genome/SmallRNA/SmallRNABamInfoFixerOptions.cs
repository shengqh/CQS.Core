using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNABamInfoFixerOptions : AbstractOptions
  {
    public SmallRNABamInfoFixerOptions()
    {
      this.PerformUpdate = false;
    }

    [Option('i', "input", MetaValue = "DIRECTORY", HelpText = "Root directory")]
    public string RootDirectory { get; set; }

    [Option("update", MetaValue = "DIRECTORY", HelpText = "Perform the update of the total count in info file with the total count from count file")]
    public bool PerformUpdate { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(RootDirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exist : {0}", RootDirectory));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
