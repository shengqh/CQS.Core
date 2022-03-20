using CommandLine;
using RCPA;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Fastq
{
  public class PairedFastqExtractorOptions : AbstractOptions
  {
    public IFilter<FastqSequence> GetFilter()
    {
      throw new NotImplementedException();
    }

    public override bool PrepareOptions()
    {
      var files = FastqFiles.TakeWhile(m => !File.Exists(m)).ToList();
      if (files.Count > 0)
      {
        ParsingErrors.Add(string.Format("Thoese files not exists {0}.", files.Merge("\n")));
        return false;
      }

      var gzipfile = new GzipTextReader(null);
      if (FastqFiles.Any(m => gzipfile.NeedProcess(m)))
      {
        if (!File.Exists(this.Gzip))
        {
          ParsingErrors.Add(string.Format("Gzip not exists {0}.", this.Gzip));
          return false;
        }
      }

      if (FastqFiles.Count != OutputFiles.Count)
      {
        ParsingErrors.Add(string.Format("Count of FastQ  not exists {0}.", this.Gzip));
        return false;
      }

      return true;
    }

    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", HelpText = "Output FastQ files")]
    public IList<string> OutputFiles { get; set; }

    [OptionList('o', "outputFiles", Required = true, MetaValue = "FILES", HelpText = "Input FastQ files")]
    public IList<string> FastqFiles { get; set; }

    [OptionList('g', "gzip", Required = false, MetaValue = "FILE", HelpText = "Gzip location")]
    public string Gzip { get; set; }
  }
}
