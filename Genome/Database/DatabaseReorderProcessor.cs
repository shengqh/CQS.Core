using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;

namespace CQS.Genome.Database
{
  public class DatabaseReorderProcessor : AbstractThreadProcessor
  {
    private DatabaseReorderProcessorOptions _options;

    public DatabaseReorderProcessor(DatabaseReorderProcessorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading sequences from: " + _options.InputFile + "...");
      var seqs = SequenceUtils.Read(_options.InputFile);

      seqs.Sort((m1, m2) =>
      {
        var chr1 = m1.Name.StringBefore("_").StringAfter("chr");
        var suffix1 = m1.Name.Contains("_") ? m1.Name.StringAfter("_") : string.Empty;
        var chr2 = m2.Name.StringBefore("_").StringAfter("chr");
        var suffix2 = m2.Name.Contains("_") ? m2.Name.StringAfter("_") : string.Empty;

        if (string.IsNullOrWhiteSpace(suffix1))
        {
          if (string.IsNullOrWhiteSpace(suffix2))
          {
            return GenomeUtils.CompareChromosome(chr1, chr2);
          }
          else
          {
            return -1;
          }
        }
        else
        {
          if (string.IsNullOrWhiteSpace(suffix2))
          {
            return 1;
          }
          else
          {
            var ret = GenomeUtils.CompareChromosome(chr1, chr2);
            if (ret == 0)
            {
              ret = suffix1.CompareTo(suffix2);
            }
            return ret;
          }
        }
      });

      Progress.SetMessage("Writing sequences to: " + _options.OutputFile + "...");
      SequenceUtils.Write(new FastaFormat(), _options.OutputFile, seqs);

      Progress.SetMessage("Finished.");

      return new[] { _options.OutputFile };
    }
  }
}
