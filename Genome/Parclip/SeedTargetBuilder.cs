using CQS.Genome;
using CQS.Genome.Bed;
using CQS.Genome.Gtf;
using CQS.Genome.Mapping;
using CQS.Genome.Mirna;
using CQS.Genome.SmallRNA;
using CQS.Genome.Feature;
using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.Parclip
{
  public class SeedTargetBuilder : AbstractTargetBuilder
  {
    private SeedTargetBuilderOptions options;

    public SeedTargetBuilder(SeedTargetBuilderOptions options)
      : base(options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var seeds = File.ReadAllLines(options.InputFile);
      var seedLengths = (from s in seeds select s.Length).Distinct().OrderBy(m => m).ToList();

      if (seedLengths.Count > 1)
      {
        throw new Exception(string.Format("The seed should be equal length, current are {0}", seedLengths.ConvertAll(m => m.ToString()).Merge(",")));
      }

      if (seedLengths.Count == 0)
      {
        throw new Exception(string.Format("No seed found in file {0}", options.InputFile));
      }

      var seedLength = seedLengths[0];

      Progress.SetMessage("Total {0} seeds readed.", seeds.Length);

      var namemap = new MapReader(1, 12).ReadFromFile(options.RefgeneFile);

      Progress.SetMessage("Build target {0} mers...", seedLength);

      //Read 6 mers from target
      var utr3seeds = BuildTargetSeeds(seedLength);
      utr3seeds.ForEach(m =>
      {
        var gene = m.Name.StringBefore("_utr3");
        m.GeneSymbol = namemap.ContainsKey(gene) ? namemap[gene] : string.Empty;
      });

      var seedMap = utr3seeds.ToGroupDictionary(m => m.Sequence.ToUpper());

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Seed\tTarget\tTargetCoverage\tTargetGeneSymbol\tTargetName");
        List<SeedItem> target;

        foreach (var seed in seeds)
        {
          if (seedMap.TryGetValue(seed, out target))
          {
            target.Sort((m1, m2) =>
            {
              return m2.Coverage.CompareTo(m1.Coverage);
            });

            for (int j = 0; j < target.Count; j++)
            {
              var t = target[j];
              sw.WriteLine("{0}\t{1}:{2}-{3}:{4}\t{5}\t{6}\t{7}",
                seed,
                t.Seqname,
                t.Start,
                t.End,
                t.Strand,
                t.Coverage,
                t.GeneSymbol,
                t.Name);
            }
          }
        }
      }

      return new[] { options.OutputFile };
    }
  }
}