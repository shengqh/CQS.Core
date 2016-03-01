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

namespace CQS.Genome.Mirna
{
  public class MirnaNTACountTableBuilder : AbstractThreadProcessor
  {
    
    private MirnaCountTableBuilderOptions options;

    public MirnaNTACountTableBuilder(MirnaCountTableBuilderOptions options)
    {
      this.options = options;
    }

    private class CountItem
    {
      public string Dir { get; set; }
      public Dictionary<string, string[]> Data { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      var countfiles = options.GetCountFiles();

      var dic = new Dictionary<string, Dictionary<string, MappedMirnaGroup>>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("Reading miRNA mapped file " + file.File + " ...");
        var mirnas = new MappedMirnaGroupXmlFileFormat().ReadFromFile(file.File);
        dic[file.Name] = mirnas.ToDictionary(m => m.DisplayName);
      }

      var features = (from c in dic.Values
                      from k in c.Keys
                      select k).Distinct().OrderBy(m => m).ToList();

      var names = dic.Keys.OrderBy(m => m).ToList();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      using (StreamWriter swNTA = new StreamWriter(options.NTAFile))
      using (StreamWriter swIso = new StreamWriter(options.IsomirFile))
      using (StreamWriter swIsoNTA = new StreamWriter(options.IsomirNTAFile))
      {

        sw.WriteLine("Feature\tLocation\tSequence\t{0}", names.Merge("\t"));
        
        swNTA.WriteLine("Feature\tLocation\tSequence\t{0}", names.Merge("\t"));

        swIso.WriteLine("Feature\tLocation\tSequence\t{0}", names.Merge("\t"));
        
        swIsoNTA.WriteLine("Feature\tLocation\tSequence\t{0}", names.Merge("\t"));

        foreach (var feature in features)
        {
          OutputCount(sw, dic, feature, names, MirnaConsts.NO_OFFSET, false, "");

          OutputCount(swNTA, dic, feature, names, MirnaConsts.NO_OFFSET, true, "");

          OutputCount(swIso, dic, feature, names, 0, false, "_+_0");
          OutputCount(swIso, dic, feature, names, 1, false, "_+_1");
          OutputCount(swIso, dic, feature, names, 2, false, "_+_2");
          
          OutputCount(swIsoNTA, dic, feature, names, 0, true, "_+_0");
          OutputCount(swIsoNTA, dic, feature, names, 1, true, "_+_1");
          OutputCount(swIsoNTA, dic, feature, names, 2, true, "_+_2");
        }
      }

      var result = new[] { options.OutputFile, options.IsomirFile, options.NTAFile, options.IsomirNTAFile }.ToList();

      return result;
    }

    private static void OutputCount(StreamWriter sw, Dictionary<string, Dictionary<string, MappedMirnaGroup>> dic, string feature, List<string> names, int offset, bool hasNTA, string indexSuffix)
    {
      var mmg = (from v in dic.Values
                 where v.ContainsKey(feature)
                 let g = v[feature]
                 from m in g
                 from mr in m.MappedRegions
                 where MirnaConsts.NO_OFFSET == offset || mr.Mapped.ContainsKey(offset)
                 select g).FirstOrDefault();

      if (mmg == null)
      {
        return;
      }

      if (!hasNTA)
      {
        sw.Write("{0}{1}\t{2}\t{3}", feature, indexSuffix, mmg.DisplayLocation, mmg[0].Sequence);
        foreach (var name in names)
        {
          var map = dic[name];
          MappedMirnaGroup group;
          if (map.TryGetValue(feature, out group))
          {
            if (MirnaConsts.NO_OFFSET == offset)
            {
              sw.Write("\t{0:0.###}", group.GetEstimatedCount());
            }
            else
            {
              sw.Write("\t{0:0.###}", group.GetEstimatedCount(offset));
            }
          }
          else
          {
            sw.Write("\t0");
          }
        }
        sw.WriteLine();
      }
      else
      {
        var ntas = (from v in dic.Values
                    where v.ContainsKey(feature)
                    let g = v[feature]
                    from m in g
                    from mr in m.MappedRegions
                    where MirnaConsts.NO_OFFSET == offset || mr.Mapped.ContainsKey(offset)
                    let offsets = (MirnaConsts.NO_OFFSET == offset) ? mr.Mapped.Keys.ToArray() : new int[] { offset }
                    from pos in offsets
                    let mapped = mr.Mapped[pos]
                    from l in mapped.AlignedLocations
                    select l.Parent.ClippedNTA).Distinct().OrderBy(m => m).ToList();

        foreach (var nta in ntas)
        {
          sw.Write("{0}{1}_NTA_{2}\t{3}\t{4}", feature, indexSuffix, nta, mmg.DisplayLocation, mmg[0].Sequence);
          foreach (var name in names)
          {
            var map = dic[name];
            MappedMirnaGroup group;
            if (map.TryGetValue(feature, out group))
            {
              sw.Write("\t{0:0.###}", group.GetEstimatedCount(offset, nta));
            }
            else
            {
              sw.Write("\t0");
            }
          }
          sw.WriteLine();
        }
      }
    }
  }
}
