using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using RCPA.Utils;

namespace CQS.Genome.Fastq
{
  public class FastqLengthDistributionBuilder : AbstractThreadProcessor
  {
    private FastqLengthDistributionBuilderOptions options;

    public FastqLengthDistributionBuilder(FastqLengthDistributionBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Dictionary<int, int> bins = new Dictionary<int, int>();
      bool bfirst = true;
      foreach (var file in options.InputFiles)
      {
        Progress.SetMessage("Processing " + file + "...");
        using (var sr = StreamUtils.GetReader(file))
        {
          int readcount = 0;
          while (!sr.EndOfStream)
          {
            var id = sr.ReadLine();
            var seq = sr.ReadLine();
            var strand = sr.ReadLine();
            var score = sr.ReadLine();
            if (string.IsNullOrEmpty(id) || seq == null)
            {
              break;
            }

            int count;
            if (!bins.TryGetValue(seq.Length, out count))
            {
              count = 0;
            }
            bins[seq.Length] = count + 1;

            if (seq.Length == 51 && bfirst)
            {
              Console.WriteLine("{0}\n{1}\n{2}\n{3}", id, seq, strand, score);
              bfirst = false;
            }

            readcount++;
            if (readcount % 100000 == 0)
            {
              Progress.SetMessage("{0} reads processed", readcount);
            }
          }
        }
      }

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        var minlen = bins.Keys.Min();
        var maxlen = bins.Keys.Max();
        for (int len = minlen + 1; len < maxlen; len++)
        {
          if (!bins.ContainsKey(len))
          {
            bins[len] = 0;
          }
        }

        var binlens = (from len in bins.Keys
                       orderby len
                       select len).ToList();
        sw.WriteLine("Len\tCount");
        foreach (var len in binlens)
        {
          sw.WriteLine("{0}\t{1}", len, bins[len]);
        }
      }

      if (SystemUtils.IsLinux)
      {
        var absoluteFile = Path.GetFullPath(options.OutputFile).Replace("\\", "/");
        var command = string.Format(@"--vanilla -e 'library(graphics);data<-read.table(""{0}"", sep=""\\t"", header=T);png(""{0}.png"", width=4000, height=3000,res=300); barplot(data[,2], names.arg=data[,1], cex.names=0.65, main=""{1}""); dev.off()'", absoluteFile, Path.GetFileName(options.InputFiles[0]));
        SystemUtils.Execute("R", command);
      }

      Progress.End();
      return new[] { options.OutputFile };
    }
  }
}
