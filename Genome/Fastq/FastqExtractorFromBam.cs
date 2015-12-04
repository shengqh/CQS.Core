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
  public class FastqExtractorFromBam : ProgressClass, IFastqExtractor
  {
    public IFilter<SAMItemSlim> Filter { get; set; }

    public FastqExtractorFromBam()
    { }

    public int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames, string countFile)
    {
      int result = 0;

      var except = new HashSet<string>(exceptQueryNames);

      SmallRNACountMap cm = new SmallRNACountMap();
      StreamWriter swCount = null;
      if (File.Exists(countFile))
      {
        var oldCm = new SmallRNACountMap(countFile);
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
          using (var sr = SAMFactory.GetReader(sourceFile, true))
          {
            string line;
            var count = 0;
            while ((line = sr.ReadLine()) != null)
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

              var ss = SAMUtils.Parse<SAMItemSlim>(line);
              ss.Qname = ss.Qname.StringBefore(SmallRNAConsts.NTA_TAG);
              if (except.Contains(ss.Qname))
              {
                continue;
              }

              if (Filter != null && !Filter.Accept(ss))
              {
                continue;
              }

              except.Add(ss.Qname);
              ss.WriteFastq(sw);

              if (swCount != null)
              {
                swCount.WriteLine("{0}\t{1}", ss.Qname, cm.Counts[ss.Qname]);
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
