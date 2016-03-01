using System.Collections.Generic;
using System.IO;
using System.Linq;

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
      var candidates = options.ReadSeeds();
      Progress.SetMessage("Total {0} seeds readed.", candidates.Length);
      var offsets = GetPossibleOffsets(string.Empty);
      Progress.SetMessage("Build target {0} mers...", options.MinimumSeedLength);

      var seeds = (from seq in candidates
                   from offset in offsets
                   select seq.Substring(offset, options.MinimumSeedLength)).ToList();
      var targetSeedMap = ParclipUtils.BuildTargetSeedMap(options, seeds, this.Progress);
      //var seeds = new HashSet<string>(from seq in candidates
      //                                from offset in offsets
      //                                select seq.Substring(offset, options.MinimumSeedLength));
      //var targetSeedMap = ParclipUtils.BuildTargetSeedMap(options, m => seeds.Contains(m.Sequence), this.Progress);

      Progress.SetMessage("Finding target...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Sequence\tSeed\tSeedOffset\tSeedLength\tTarget\tTargetCoverage\tTargetGeneSymbol\tTargetName");

        foreach (var seq in candidates)
        {
          foreach (var offset in offsets)
          {
            if (seq.Length < offset + options.MinimumSeedLength)
            {
              break;
            }

            var seed = seq.Substring(offset, options.MinimumSeedLength);

            List<SeedItem> target;

            if (targetSeedMap.TryGetValue(seed, out target))
            {
              if (target.ConvertAll(l => l.Coverage).Distinct().Count() == 1)
              {
                GenomeUtils.SortChromosome(target, m => m.Seqname, m => m.Start);
              }
              else
              {
                target.Sort((m1, m2) =>
                {
                  return m2.Coverage.CompareTo(m1.Coverage);
                });
              }

              var longest = ParclipUtils.ExtendToLongestTarget(target, null, seq, offset, options.MinimumSeedLength, int.MaxValue, options.MinimumCoverage);

              for (int j = 0; j < longest.Count; j++)
              {
                var t = longest[j];
                var finalSeed = seq.Substring(offset, (int)t.Length);
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}:{5}-{6}:{7}\t{8}\t{9}\t{10}",
                  seq,
                  finalSeed,
                  offset,
                  finalSeed.Length,
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
      }

      return new[] { options.OutputFile };
    }
  }
}