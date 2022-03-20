using CommandLine;
using RCPA.Commandline;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Annotation
{
  public class ParalyzerClusterAnnotatorOptions : AbstractOptions
  {
    public ParalyzerClusterAnnotatorOptions()
    { }

    [Option('i', "input", Required = true, MetaValue = "FILE", HelpText = "Input cluster file")]
    public string InputFile { get; set; }

    [OptionList('c', "coordinates", Required = true, Separator = ',', MetaValue = "FILE", HelpText = "Coordinates files (gtf or bed format)")]
    public IList<string> CoordinateFiles { get; set; }

    [OptionList('f', "features", Required = true, Separator = ',', HelpText = "Features in gtf file, separated by ','")]
    public IList<string> Features { get; set; }

    [OptionList('n', "namekey", DefaultValue = "Name", HelpText = "Name key in description")]
    public string NameKey { get; set; }

    [Option('o', "output", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      foreach (var corfile in CoordinateFiles)
      {
        if (!File.Exists(corfile))
        {
          ParsingErrors.Add(string.Format("Coordinate file not exists {0}.", corfile));
          return false;
        }
      }

      if (Features == null)
      {
        Features = new List<string>();
      }

      return true;
    }
  }
}
