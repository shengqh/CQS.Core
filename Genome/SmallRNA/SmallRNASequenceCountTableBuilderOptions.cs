using CommandLine;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    private const int DEFAULT_TOP_NUMBER = 100;
    private const int DEFAULT_ExportFastaNumber = 100;

    public SmallRNASequenceCountTableBuilderOptions()
    {
      this.TopNumber = DEFAULT_TOP_NUMBER;
      this.ExportFasta = false;
      this.ExportFastaNumber = 100;
    }

    [Option('n', "number", DefaultValue = DEFAULT_TOP_NUMBER, MetaValue = "INT", HelpText = "Select top X reads in each file")]
    public int TopNumber { get; set; }

    [Option("exportFasta", MetaValue = "BOOLEAN", HelpText = "Export reads as fasta format for blast")]
    public bool ExportFasta { get; set; }

    [Option("exportFastaNumber", DefaultValue = DEFAULT_ExportFastaNumber, MetaValue = "INT", HelpText = "The total number of reads to be exported as fasta")]
    public int ExportFastaNumber { get; set; }

    protected override void ValidateListFile()
    {
      if (!File.Exists(this.ListFile))
      {
        ParsingErrors.Add(string.Format("List file not exists {0}.", this.ListFile));
      }
      else
      {
        var files = (from l in File.ReadAllLines(this.ListFile)
                     where l.Trim().Length > 1
                     select l).ToList();

        foreach (var file in files)
        {
          var parts = file.Split('\t');
          var name = parts[0];
          var countFile = parts[1];
          if (!File.Exists(countFile))
          {
            ParsingErrors.Add(string.Format("Count file not exists {0}.", countFile));
          }

          if(parts.Length > 2)
          {
            var fastqFile = parts[2];
            if (!File.Exists(fastqFile))
            {
              ParsingErrors.Add(string.Format("Fastq file not exists {0}.", fastqFile));
            }
          }
        }
      }
    }
  }
}
