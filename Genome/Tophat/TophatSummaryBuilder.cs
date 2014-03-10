using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Tophat
{
  public class TophatSummaryBuilder : AbstractThreadProcessor
  {
    private TophatSummaryBuilderOptions _options;
    public TophatSummaryBuilder(TophatSummaryBuilderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      var subdirs = (from dir in Directory.GetDirectories(_options.InputDir)
                     let summaryfile = Path.Combine(dir, "align_summary.txt")
                     where File.Exists(summaryfile)
                     select new
                     {
                       Name = _options.Prefix + Path.GetFileName(dir),
                       File = summaryfile
                     }).ToList();

      using (StreamWriter sw = new StreamWriter(_options.OutputFile))
      {
        sw.WriteLine("Name\tLeftReads\tLeftReadsMapped\tLeftMappedPercentage\tLeftMappedMulti\tRightReads\tRightReadsMapped\tRightMappedPercentage\tRightMappedMulti\tOverallMappedRate\tAlignedPairs\tAlignedPairsMulti\tAlignedPairDiscordant");
        foreach (var dir in subdirs)
        {
          var lines = File.ReadAllLines(dir.File);
          sw.WriteLine("{0}\t{1}\t{2}\t{3}%\t{4}%\t{5}\t{6}\t{7}%\t{8}%\t{9}%\t{10}\t{11}%\t{12}%",
            dir.Name,
            lines[1].StringAfter(":").Trim(),
            lines[2].StringAfter(":").StringBefore("(").Trim(),
            lines[2].StringAfter("(").StringBefore("%").Trim(),
            lines[3].StringAfter("(").StringBefore("%").Trim(),
            lines[5].StringAfter(":").Trim(),
            lines[6].StringAfter(":").StringBefore("(").Trim(),
            lines[6].StringAfter("(").StringBefore("%").Trim(),
            lines[7].StringAfter("(").StringBefore("%").Trim(),
            lines[8].StringBefore("%").Trim(),
            lines[10].StringAfter(":").Trim(),
            lines[11].StringAfter("(").StringBefore("%").Trim(),
            lines[12].StringAfter("(").StringBefore("%").Trim());
        }
      }

      return new[] { _options.OutputFile };
    }
  }
}
