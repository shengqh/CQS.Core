using Bio.IO.SAM;
using CQS.Genome.Sam;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class SequenceCountTableBuilder : AbstractThreadProcessor
  {
    private SequenceCountTableBuilderOptions options;

    public SequenceCountTableBuilder(SequenceCountTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var countFiles = options.GetCountFiles();
      countFiles.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

      var countMap = new Dictionary<string, Dictionary<string, int>>();
      int fileIndex = 0;
      foreach (var file in countFiles)
      {
        fileIndex++;
        Progress.SetMessage("Reading {0}/{1}: {2} ...", fileIndex, countFiles.Count, file.File);

        var queries = new HashSet<string>();
        using (var sr = SAMFactory.GetReader(file.File, true))
        {
          int count = 0;
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            count++;

            if (count % 1000 == 0)
            {
              if (Progress.IsCancellationPending())
              {
                throw new UserTerminatedException();
              }
            }

            var parts = line.Split('\t');

            SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);

            //unmatched
            if (flag.HasFlag(SAMFlags.UnmappedQuery))
            {
              continue;
            }

            queries.Add(parts[SAMFormatConst.QNAME_INDEX]);
          }
        }

        var countDic = new Dictionary<string, int>();
        countMap[file.Name] = countDic;
        var cm = new MapItemReader(0, 1, informationIndex: 2).ReadFromFile(file.AdditionalFile);
        foreach (var query in queries)
        {
          var count = cm[query];
          countDic[count.Information] = int.Parse(count.Value);
        }

        Progress.SetMessage("{0} reads mapped.", queries.Count);
      }

      var uniques = (from c in countMap.Values
                     from seq in c.Keys
                     select seq).Distinct().ToArray();
      var uniqueCounts = (from seq in uniques
                          let totalCount = (from c in countMap.Values
                                            where c.ContainsKey(seq)
                                            select c[seq]).Sum()
                          select new { Sequence = seq, Count = totalCount }).OrderByDescending(m => m.Count).ToArray();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Sequence\t" + (from cf in countFiles select cf.Name).Merge("\t"));
        foreach (var uc in uniqueCounts)
        {
          var seq = uc.Sequence;
          sw.Write(seq);
          foreach (var cf in countFiles)
          {
            var map = countMap[cf.Name];
            int count;
            if (map.TryGetValue(seq, out count))
            {
              sw.Write("\t{0}", count);
            }
            else
            {
              sw.Write("\t0");
            }
          }
          sw.WriteLine();
        }
      }

      Progress.End();

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
