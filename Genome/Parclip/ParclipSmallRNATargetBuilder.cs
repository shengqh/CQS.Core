using CQS.Genome.SmallRNA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNATargetBuilder : AbstractTargetBuilder
  {
    private ParclipSmallRNATargetBuilderOptions options;

    public ParclipSmallRNATargetBuilder(ParclipSmallRNATargetBuilderOptions options)
      : base(options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading smallRNA...");

      //exclude lincRNA
      var mappedSmallRNA = ParclipUtils.GetSmallRNACoverageRegion(options.InputFile, includeSmallRNATags: null, excudeSmallRNATags: SmallRNAConsts.lncRNA);
      mappedSmallRNA.Sort((m1, m2) => m2.Coverages.Average(l => l.Coverage).CompareTo(m1.Coverages.Average(l => l.Coverage)));

      Progress.SetMessage("Build target {0} mers...", options.MinimumSeedLength);
      var targetSeedMap = ParclipUtils.BuildTargetSeedMap(options, m => true, progress: this.Progress);

      Progress.SetMessage("Finding target...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("SmallRNA\tChr\tStart\tEnd\tStrand\tSeed\tSeedOffset\tSeedLength\tSeedCoverage\tTarget\tTargetCoverage\tTargetGeneSymbol\tTargetName\tTargetSeedOffset\tTargetSeedUniqueRead");

        foreach (var t2c in mappedSmallRNA)
        {
          var seq = t2c.Sequence.ToUpper();

          int[] offsets = GetPossibleOffsets(t2c.Name);

          foreach (var offset in offsets)
          {
            if (offset + options.MinimumSeedLength >= seq.Length)
            {
              Console.Error.WriteLine("t2c={0}/{1}, seq={2}, offset={3}, minseed={4}", t2c.Name, t2c.GeneSymbol, seq, offset, options.MinimumSeedLength);
              continue;
            }

            var seed = seq.Substring(offset, options.MinimumSeedLength);
            var coverage = t2c.Coverages.Skip(offset).Take(options.MinimumSeedLength).Average(l => l.Coverage);
            if (coverage < options.MinimumCoverage)
            {
              continue;
            }

            List<SeedItem> target;
            if (targetSeedMap.TryGetValue(seed, out target))
            {
              target.Sort((m1, m2) =>
              {
                return m2.Coverage.CompareTo(m1.Coverage);
              });

              var longest = ParclipUtils.FindLongestTarget(target, t2c, seq, offset, options.MinimumSeedLength, int.MaxValue, options.MinimumCoverage);

              for (int j = 0; j < longest.Count; j++)
              {
                var t = longest[j];
                var finalSeed = seq.Substring(offset, (int)t.Length);

                sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", t2c.Name, t2c.Seqname, t2c.Start, t2c.End, t2c.Strand, finalSeed, offset, finalSeed.Length, Math.Round(coverage));

                var uniqueReadCount = (from c in t.Source.Coverages.Skip(t.SourceOffset).Take(finalSeed.Length)
                                       from q in c.UniqueRead
                                       select q).Distinct().Count();

                sw.WriteLine("\t{0}:{1}-{2}:{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                  t.Seqname,
                  t.Start,
                  t.End,
                  t.Strand,
                  t.Coverage,
                  t.GeneSymbol,
                  t.Name,
                  t.SourceOffset,
                  t.GetSeedUniqueReadCount(finalSeed.Length));
              }
            }
          }
        }
      }

      return new[] { options.OutputFile };
    }
  }
}