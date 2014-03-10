using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome.Sam;
using RCPA.Gui;
using Bio.IO.SAM;
using RCPA;

namespace CQS.Genome.Fastq
{
  public class FastqExtractorFromBam : ProgressClass, IFastqExtractor
  {
    public IFilter<SAMItemSlim> Filter { get; set; }

    private string samtools;

    public FastqExtractorFromBam(string samtools)
    {
      this.samtools = samtools;
    }

    public int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames)
    {
      int result = 0;

      var except = new HashSet<string>(exceptQueryNames);

      using (StreamWriter sw = new StreamWriter(targetFile))
      {
        using (var sr = SAMFactory.GetReader(sourceFile, samtools, true))
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
            result++;
          }
        }
      }
      return result;
    }
  }
}
