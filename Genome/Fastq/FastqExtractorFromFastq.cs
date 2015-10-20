using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome.Sam;
using RCPA.Gui;
using Bio.IO.SAM;
using RCPA;
using CQS.Genome.Mirna;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Fastq
{
  public class FastqExtractorFromFastq : ProgressClass, IFastqExtractor
  {
    public IFilter<FastqSequence> Filter { get; set; }

    public int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames, string countFile)
    {
      int result = 0;

      var except = new HashSet<string>(exceptQueryNames);

      CountMap cm = new CountMap();
      StreamWriter swCount = null;
      if (File.Exists(countFile))
      {
        var oldCm = new CountMap(countFile);
        foreach (var c in oldCm.Counts)
        {
          cm.Counts[c.Key.StringBefore(SmallRNAConsts.NTA_TAG)] = c.Value;
        }
        swCount = new StreamWriter(targetFile + ".dupcount");
      }

      try
      {
        using (var sw = StreamUtils.GetWriter(targetFile, targetFile.ToLower().EndsWith(".gz")))
        {
          using (var sr = StreamUtils.GetReader(sourceFile))
          {
            FastqReader reader = new FastqReader();
            FastqWriter writer = new FastqWriter();

            FastqSequence ss;
            var count = 0;
            while ((ss = reader.Parse(sr)) != null)
            {
              count++;

              if (count % 100000 == 0)
              {
                Progress.SetMessage("{0} reads", count);
                if (Progress.IsCancellationPending())
                {
                  throw new UserTerminatedException();
                }
              }

              ss.Reference = ss.Name.StringBefore(SmallRNAConsts.NTA_TAG) + " " + ss.Description;
              if (except.Contains(ss.Name))
              {
                continue;
              }

              if (Filter != null && !Filter.Accept(ss))
              {
                continue;
              }

              except.Add(ss.Name);
              writer.Write(sw, ss);

              if (swCount != null)
              {
                swCount.WriteLine("{0}\t{1}", ss.Name, cm.Counts[ss.Name]);
              }

              result++;
            }
          }
        }
      }
      finally
      {
        if (swCount != null)
        {
          swCount.Close();
        }
      }
      return result;
    }
  }
}
