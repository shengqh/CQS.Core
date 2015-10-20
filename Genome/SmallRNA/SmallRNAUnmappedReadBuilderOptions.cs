using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using RCPA.Seq;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAUnmappedReadBuilderOptions : AbstractOptions
  {
    public SmallRNAUnmappedReadBuilderOptions()
    { }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Original fastq file")]
    public string InputFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('x', "xmlFile", Required = true, MetaValue = "FILE", HelpText = "Mapped smallRNA XML file. The reads mapped to smallRNA will be excluded.")]
    public string XmlFile { get; set; }

    [Option('e', "excludeFile", Required = false, MetaValue = "FILE", HelpText = "Exclude read name file. The reads in this file will be excluded.")]
    public string ExcludeFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output unmapped fastq file")]
    public string OutputFile { get; set; }

    [Option('s', "excludeBySequence", Required = false, HelpText = "Exclude reads by read sequence")]
    public bool ExcludeBySequence { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
      }

      if (!File.Exists(this.XmlFile))
      {
        ParsingErrors.Add(string.Format("Mapped smallRNA XML file not exists {0}.", this.XmlFile));
      }

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.ExcludeFile) && !File.Exists(this.ExcludeFile))
      {
        ParsingErrors.Add(string.Format("Exclude file not exists {0}.", this.ExcludeFile));
      }

      if (ExcludeBySequence)
      {
        if (!File.Exists(this.CountFile))
        {
          ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
        }

        if (!File.Exists(this.XmlFile))
        {
          ParsingErrors.Add(string.Format("Mapped smallRNA XML file not exists {0}.", this.XmlFile));
        }
      }

      return ParsingErrors.Count == 0;
    }

    private CountMap cm;

    public virtual CountMap GetCountMap()
    {
      if (cm == null)
      {
        cm = new CountMap(this.CountFile);
      }
      return cm;
    }
  }
}
