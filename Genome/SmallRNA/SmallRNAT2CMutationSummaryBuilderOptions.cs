using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationSummaryBuilderOptions : AbstractOptions
  {
    [Option('l', "list", Required = true, MetaValue = "FILE", HelpText = "File contains count xml files, one file per line")]
    public string ListFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public List<FileItem> GetCountXmlFiles()
    {
      return (from file in File.ReadAllLines(this.ListFile)
              where file.Trim().Length > 0
              let parts = file.Split('\t')
              let namefile = parts.Length == 1
              ? new FileItem()
              {
                Name = Path.GetFileName(file).StringBefore("."),
                File = file
              } : new FileItem()
              {
                Name = parts[0],
                File = parts[1]
              }
              select namefile).ToList();
    }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.ListFile))
      {
        ParsingErrors.Add(string.Format("List file not exists {0}.", this.ListFile));
      }
      else
      {
        var files = GetCountXmlFiles();
        foreach (var file in files)
        {
          if (!File.Exists(file.File))
          {
            ParsingErrors.Add(string.Format("Count xml file not exists {0}.", file.File));
          }
        }
      }

      return ParsingErrors.Count == 0;
    }
  }
}
