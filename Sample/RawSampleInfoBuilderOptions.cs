using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Sample
{
  public class RawSampleInfoBuilderOptions : AbstractOptions
  {
    public RawSampleInfoBuilderOptions()
    { }

    [Option('i', "inputDirectory", Required = true, MetaValue = "DIRECTORY", HelpText = "Input directory which contains sub directories with .siformat file (property definition file)")]
    public string InputDirectory { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDirectory))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDirectory));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
