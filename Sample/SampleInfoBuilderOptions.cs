using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Sample
{
  public class SampleInfoBuilderOptions : AbstractOptions
  {
    public SampleInfoBuilderOptions()
    { }

    [Option('i', "inputDirectory", Required = true, MetaValue = "DIRECTORY", HelpText = "Input directory which contains sub directories with .siformat file (property definition file)")]
    public string InputDirectory { get; set; }

    [Option('p', "propertyFile", Required = true, MetaValue = "FILE", HelpText = "Property file, each line contains one property")]
    public string PropertyFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDirectory))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDirectory));
      }

      if (!File.Exists(this.PropertyFile))
      {
        ParsingErrors.Add(string.Format("Property file not exists {0}.", this.PropertyFile));
      }

      try
      {
        SampleDirectories();
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(string.Format("Directory wrong: {0}.", ex.Message));
      }

      return ParsingErrors.Count == 0;
    }

    public string[] SampleDirectories()
    {
      if (File.Exists(Path.Combine(InputDirectory, Path.GetFileName(InputDirectory) + ".siformat")))
      {
        return new[] { this.InputDirectory };
      }

      var result = SampleUtils.GetDatasets(InputDirectory);
      foreach (var dir in result)
      {
        var sfile = Path.Combine(dir, Path.GetFileName(dir) + ".siformat");
        if (!File.Exists(sfile))
        {
          throw new Exception("Cannot find file :" + sfile);
        }
      }

      return result;
    }
  }
}
