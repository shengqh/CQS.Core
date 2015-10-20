using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using Bio.IO.SAM;
using CQS.Genome.Bed;
using CQS.Genome.Fastq;
using System.Collections.Concurrent;
using System.Threading;
using RCPA.Commandline;
using CommandLine;
using System.Text.RegularExpressions;
using CQS.Genome.Mapping;
using CQS.Genome.Feature;
using CQS.Genome.Mirna;

namespace CQS.Genome.SmallRNA
{
  public class MirnaNTACountTableWriter : SmallRNACountTableWriter
  {
    protected override List<string> DoWriteToFile(string outputFile, Dictionary<string, Dictionary<string, FeatureItemGroup>> dic, List<string> samples, List<string> orderedFeatures, string removeNamePrefix)
    {
      var result = base.DoWriteToFile(outputFile, dic, samples, orderedFeatures, removeNamePrefix);

      var ntaFile = Path.ChangeExtension(outputFile, ".NTA.count");
      var isomiRFile = Path.ChangeExtension(outputFile, ".isomiR.count");
      var isomiRntaFile = Path.ChangeExtension(outputFile, ".isomiR_NTA.count");
      using (StreamWriter swNTA = new StreamWriter(ntaFile))
      using (StreamWriter swIso = new StreamWriter(isomiRFile))
      using (StreamWriter swIsoNTA = new StreamWriter(isomiRntaFile))
      {
        swNTA.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        swIso.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        swIsoNTA.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));

        foreach (var feature in orderedFeatures)
        {
          OutputCount(swNTA, dic, feature, samples, MirnaConsts.NO_OFFSET, true, "", removeNamePrefix);

          OutputCount(swIso, dic, feature, samples, 0, false, "_+_0", removeNamePrefix);
          OutputCount(swIso, dic, feature, samples, 1, false, "_+_1", removeNamePrefix);
          OutputCount(swIso, dic, feature, samples, 2, false, "_+_2", removeNamePrefix);

          OutputCount(swIsoNTA, dic, feature, samples, 0, true, "_+_0", removeNamePrefix);
          OutputCount(swIsoNTA, dic, feature, samples, 1, true, "_+_1", removeNamePrefix);
          OutputCount(swIsoNTA, dic, feature, samples, 2, true, "_+_2", removeNamePrefix);
        }
      }

      result.Add(ntaFile);
      result.Add(isomiRFile);
      result.Add(isomiRntaFile);

      return result;
    }

    public override IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, List<string> samples, string removeNamePrefix)
    {
      var result = base.WriteToFile(outputFile, features, samples, removeNamePrefix).ToList();

      var ntaFile = Path.ChangeExtension(outputFile, ".NTA.count");
      var isomiRFile = Path.ChangeExtension(outputFile, ".isomiR.count");
      var isomiRntaFile = Path.ChangeExtension(outputFile, ".isomiR_NTA.count");
      using (StreamWriter swNTA = new StreamWriter(ntaFile))
      using (StreamWriter swIso = new StreamWriter(isomiRFile))
      using (StreamWriter swIsoNTA = new StreamWriter(isomiRntaFile))
      {
        swNTA.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        swIso.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));
        swIsoNTA.WriteLine("Feature\tLocation\tSequence\t{0}", samples.Merge("\t"));

        foreach (var feature in features)
        {
          OutputCount(swNTA, feature, samples, MirnaConsts.NO_OFFSET, true, "", removeNamePrefix);

          OutputCount(swIso, feature, samples, 0, false, "_+_0", removeNamePrefix);
          OutputCount(swIso, feature, samples, 1, false, "_+_1", removeNamePrefix);
          OutputCount(swIso, feature, samples, 2, false, "_+_2", removeNamePrefix);

          OutputCount(swIsoNTA, feature, samples, 0, true, "_+_0", removeNamePrefix);
          OutputCount(swIsoNTA, feature, samples, 1, true, "_+_1", removeNamePrefix);
          OutputCount(swIsoNTA, feature, samples, 2, true, "_+_2", removeNamePrefix);
        }
      }

      result.Add(ntaFile);
      result.Add(isomiRFile);
      result.Add(isomiRntaFile);

      return result;
    }
  }
}
