using CQS.Genome.Pileup;
using CQS.Genome.Samtools;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public class ExtractProcessor : AbstractThreadProcessor
  {
    private ExtractProcessorOptions options;
    public ExtractProcessor(ExtractProcessorOptions options)
    {
      this.options = options;
    }

    protected static string GetLinuxFile(string filename)
    {
      return Path.GetFullPath(filename).Replace("\\", "/");
    }

    public override IEnumerable<string> Process()
    {
      options.PrintParameter(Console.Out);

      Progress.SetMessage("Single thread mode ...");
      var parser = options.GetPileupItemParser();
      var pfile = new PileupFile(parser);

      var mutationList = new ValidationFile().ReadFromFile(options.BedFile);
      var map = mutationList.Items.ToDictionary(m => GenomeUtils.GetKey(m.Chr, m.Pos));

      var posFile = Path.Combine(options.OutputFile + ".pos.bed");
      mutationList.WriteToFile(posFile, 500);
      var proc = new MpileupProcessor(options).ExecuteSamtools(options.BamFiles, "", posFile);
      if (proc == null)
      {
        return null;
      }

      pfile.Open(proc.StandardOutput);
      pfile.Samtools = proc;

      Progress.SetMessage("Total {0} entries in extraction list", mutationList.Items.Length);

      var result = new Dictionary<ValidationItem, PileupItem>();

      using (pfile)
      {
        try
        {
          string line;
          string lastChrom = string.Empty;
          while ((line = pfile.ReadLine()) != null)
          {
            try
            {
              var locus = parser.GetSequenceIdentifierAndPosition(line);
              var locusKey = GenomeUtils.GetKey(locus.SequenceIdentifier, locus.Position);

              if (!locus.SequenceIdentifier.Equals(lastChrom))
              {
                Progress.SetMessage("Processing chromosome " + locus.SequenceIdentifier + " ...");
                lastChrom = locus.SequenceIdentifier;
              }

              ValidationItem vitem = null;
              if (!map.TryGetValue(locusKey, out vitem))
              {
                continue;
              }

              result[vitem] = parser.GetValue(line);
            }
            catch (Exception ex)
            {
              var error = string.Format("Parsing error {0}\n{1}", ex.Message, line);
              Progress.SetMessage(error);
              Console.Error.WriteLine(ex.StackTrace);
              throw new Exception(error);
            }
          }
        }
        finally
        {
          if (pfile.Samtools != null)
          {
            try
            {
              pfile.Samtools.Kill();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
          }
        }
      }

      if (result.Count == 0)
      {
        throw new Exception("Nothing found. Look at the log file for error please.");
      }

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("{0}\t{1}", mutationList.Header, options.GetBamNames().Merge("\t"));
        var emptyevents = new string('\t', options.BamFiles.Count);
        foreach (var mu in mutationList.Items)
        {
          sw.Write("{0}", mu.Line);
          PileupItem item;
          if (result.TryGetValue(mu, out item))
          {
            foreach (var sample in item.Samples)
            {
              sample.InitEventCountList(false);
              if (sample.EventCountList.Count > 0)
              {
                sw.Write("\t{0}", (from ecl in sample.EventCountList
                                   let v = string.Format("{0}:{1}", ecl.Event, ecl.Count)
                                   select v).Merge(","));
              }
              else
              {
                sw.Write("\t");
              }
            }
          }
          else
          {
            sw.Write(emptyevents);
          }
          sw.WriteLine();
        }
      }

      return new[] { options.OutputFile };
    }

  }
}