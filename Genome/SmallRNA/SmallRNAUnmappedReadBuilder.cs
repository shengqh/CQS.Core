using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;
using CQS.Genome.Mirna;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using RCPA.Utils;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAUnmappedReadBuilder : AbstractThreadProcessor
  {
    public Func<FastqSequence, bool> Accept { get; set; }

    private SmallRNAUnmappedReadBuilderOptions options;

    public SmallRNAUnmappedReadBuilder(SmallRNAUnmappedReadBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var except = new HashSet<string>();
      if (File.Exists(options.XmlFile))
      {
        //exclude the reads mapped to features no matter how many number of mismatch it has
        var allmapped = new FeatureItemGroupXmlFormat().ReadFromFile(options.XmlFile);
        except.UnionWith(from g in allmapped
                         from f in g
                         from l in f.Locations
                         from sl in l.SamLocations
                         select sl.SamLocation.Parent.Qname.StringBefore(SmallRNAConsts.NTA_TAG));
      }

      if (File.Exists(options.ExcludeFile))
      {
        except.UnionWith(from l in File.ReadAllLines(options.ExcludeFile)
                         select l.StringBefore(SmallRNAConsts.NTA_TAG));
      }

      CountMap cm = options.GetCountMap();
      var keys = cm.Counts.Keys.Where(m => m.Contains(SmallRNAConsts.NTA_TAG)).ToArray();
      foreach (var key in keys)
      {
        cm.Counts[key.StringBefore(SmallRNAConsts.NTA_TAG)] = cm.Counts[key];
      }
      StreamWriter swCount = null;
      if (File.Exists(options.CountFile))
      {
        swCount = new StreamWriter(options.OutputFile + ".dupcount");
      }

      Progress.SetMessage("output unmapped query...");
      try
      {
        using (var sw = StreamUtils.GetWriter(options.OutputFile, options.OutputFile.ToLower().EndsWith(".gz")))
        {
          using (var sr = StreamUtils.GetReader(options.InputFile))
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

              if (Accept != null && !Accept(ss))
              {
                continue;
              }

              except.Add(ss.Name);
              writer.Write(sw, ss);

              if (swCount != null)
              {
                int cmcount;
                if (!cm.Counts.TryGetValue(ss.Name, out cmcount))
                {
                  throw new Exception(string.Format("Cannot find {0} in count map", ss.Name));
                }
                swCount.WriteLine("{0}\t{1}", ss.Name, cmcount);
              }
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

      Progress.End();

      return result;
    }
  }
}