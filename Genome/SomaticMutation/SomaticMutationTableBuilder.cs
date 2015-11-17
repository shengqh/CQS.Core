using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.SomaticMutation
{
  public class SomaticMutationTableBuilder : AbstractThreadProcessor
  {
    private SomaticMutationTableBuilderOptions options;

    public SomaticMutationTableBuilder(SomaticMutationTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var itemMap = new Dictionary<string, Dictionary<string, SomaticItem>>();
      var files = (from line in File.ReadAllLines(options.InputFile)
                   where !string.IsNullOrWhiteSpace(line)
                   let parts = line.Split('\t')
                   select new { Key = parts[0], File = parts[1] }).ToList();
      if (files.Count > 0 && !File.Exists(files[0].File))
      {//maybe header
        files.RemoveAt(0);
      }


      foreach (var file in files)
      {
        var items = SomaticMutationUtils.ParseGlmvcFile(file.File, options.AcceptChromosome);
        itemMap[file.Key] = items.ToDictionary(m => m.Key);
      }

      using (var sw = new StreamWriter(options.OutputFile))
      {
        var samples = itemMap.Keys.OrderBy(m => m).ToArray();

        List<Tuple<string, Func<SomaticItem, string>>> funcs = new List<Tuple<string, Func<SomaticItem, string>>>();
        funcs.Add(new Tuple<string, Func<SomaticItem, string>>("#chr", m => m.Chrom));
        funcs.Add(new Tuple<string, Func<SomaticItem, string>>("start", m => m.StartPosition.ToString()));
        funcs.Add(new Tuple<string, Func<SomaticItem, string>>("end", m => m.StartPosition.ToString()));

        if (itemMap.Values.Any(m => m.Values.Any(l => !string.IsNullOrWhiteSpace(l.RefGeneName))))
        {
          funcs.Add(new Tuple<string, Func<SomaticItem, string>>("gene", m => m.RefGeneName));
          funcs.Add(new Tuple<string, Func<SomaticItem, string>>("func", m => m.RefGeneFunc));
          funcs.Add(new Tuple<string, Func<SomaticItem, string>>("exonic_func", m => m.RefGeneExonicFunc));
          funcs.Add(new Tuple<string, Func<SomaticItem, string>>("aa_change", m => m.RefGeneAAChange));
        }

        sw.Write(funcs.ConvertAll(l => l.Item1).Merge("\t"));
        foreach (var sample in samples)
        {
          sw.Write("\t{0}", sample);
        }
        sw.WriteLine("\tDetectedTimes");

        var locus = (from v in itemMap.Values from vv in v.Keys select vv).Distinct().ToList();
        GenomeUtils.SortChromosome(locus, m => m.StringBefore("_"), m => int.Parse(m.StringAfter("_")));

        foreach (var loc in locus)
        {
          var item = (from v in itemMap.Values from vv in v where vv.Key.Equals(loc) select vv.Value).First();
          sw.Write("{0}", funcs.ConvertAll(l => l.Item2(item)).Merge("\t"));
          var count = 0;
          foreach (var sample in samples)
          {
            SomaticItem curitem;
            if (itemMap[sample].TryGetValue(loc, out curitem))
            {
              sw.Write("\t{0}/{1}={2}/{3}|{4}/{5}", curitem.RefAllele, curitem.AltAllele, curitem.NormalMajorCount, curitem.NormalMinorCount, curitem.TumorMajorCount, curitem.TumorMinorCount);
              count++;
            }
            else
            {
              sw.Write("\t");
            }
          }
          sw.WriteLine("\t{0}", count);
        }
      }

      return new[] { options.OutputFile };
    }
  }
}
