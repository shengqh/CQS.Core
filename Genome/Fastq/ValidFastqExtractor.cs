using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;

namespace CQS.Genome.Fastq
{
  public class ValidFastqExtractor : AbstractThreadProcessor
  {
    private ValidFastqExtractorOptions options;

    public ValidFastqExtractor(ValidFastqExtractorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Processing " + options.InputFile + " and writing to " + options.OutputFile + "...");

      List<string> fastq = new List<string>();

      using (var sr = StreamUtils.GetReader(options.InputFile))
      {
        using (var sw = StreamUtils.GetWriter(options.OutputFile, options.OutputFile.ToLower().EndsWith(".gz")))
        using (var swUnvalid = StreamUtils.GetWriter(options.OutputFile + ".unvalid", false))
        {
          string line;
          int validCount = 0;
          int unvalidCount = 0;
          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith("@"))
            {
              if (fastq.Count == 3 && fastq[1].Length == line.Length) // if it's the score line
              {
                fastq.Add(line);
                continue;
              }

              if (IsValid(fastq))
              {
                validCount++;
                fastq.ForEach(m => sw.WriteLine(m));
              }
              else if (fastq.Count > 0)
              {
                swUnvalid.WriteLine("--unvalid--");
                fastq.ForEach(m => swUnvalid.WriteLine(m));
                unvalidCount++;
              }
              fastq.Clear();

              if (validCount % 100000 == 0)
              {
                if (Progress.IsCancellationPending())
                {
                  throw new UserTerminatedException();
                }

                Progress.SetMessage("Valid = {0}, unvalid = {1}", validCount, unvalidCount);
              }
            }
            fastq.Add(line);
          }

          if (IsValid(fastq))
          {
            validCount++;
            fastq.ForEach(m => sw.WriteLine(m));
          }
          else if (fastq.Count > 0)
          {
            swUnvalid.WriteLine("--unvalid--");
            fastq.ForEach(m => swUnvalid.WriteLine(m));
            unvalidCount++;
          }

          swUnvalid.WriteLine("--summary--");
          swUnvalid.WriteLine("valid = {0}, unvalid = {1}", validCount, unvalidCount);
          Progress.SetMessage("Finally, valid = {0}, unvalid = {1}", validCount, unvalidCount);
        }
      }
      Progress.End();

      return new string[] { options.OutputFile };
    }

    private bool IsValid(List<string> fastq)
    {
      if (fastq.Count < 4)
      {
        return false;
      }

      if (fastq[1].Length != fastq[3].Length || !fastq[2].StartsWith("+"))
      {
        return false;
      }

      if (fastq.Count > 4)
      {
        fastq.RemoveRange(4, fastq.Count - 4);
      }

      return true;
    }
  }
}
