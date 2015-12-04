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
  public class ParclipSmallRNATargetDirectBuilder : AbstractTargetBuilder
  {
    private ParclipSmallRNATargetBuilderOptions options;

    public ParclipSmallRNATargetDirectBuilder(ParclipSmallRNATargetBuilderOptions options)
      : base(options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading T2C smallRNA...");
      var mappedSmallRNA = ParclipUtils.GetSmallRNACoverageRegion(options.InputFile, null, new string[] { SmallRNAConsts.lincRNA });
      mappedSmallRNA.Sort((m1, m2) => m2.Coverages.Average().CompareTo(m1.Coverages.Average()));

      Progress.SetMessage("Build target {0} mers...", options.MinimumSeedLength);
      var targetSeedMap = ParclipUtils.BuildTargetSeedMap(options, m => true, progress: this.Progress);

      Progress.SetMessage("Finding target...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("SmallRNA\tChr\tStart\tEnd\tStrand\tSeed\tSeedOffset\tSeedLength\tSeedCoverage\tTarget\tTargetCoverage\tTargetGeneSymbol\tTargetName");

        foreach (var t2c in mappedSmallRNA)
        {
          var seq = t2c.Sequence.ToUpper();

          int[] offsets = GetPossibleOffsets(t2c.Name);

          foreach (var offset in offsets)
          {
            var seed = seq.Substring(offset, options.MinimumSeedLength);
            var coverage = t2c.Coverages.Skip(offset).Take(options.MinimumSeedLength).Average();
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

              if (!t2c.Name.StartsWith(SmallRNAConsts.miRNA))
              {
                target = ParclipUtils.FindLongestTarget(target, t2c, seq, offset, options.MinimumSeedLength, int.MaxValue, options.MinimumCoverage);
              }

              for (int j = 0; j < target.Count; j++)
              {
                var finalSeed = seq.Substring(offset, target[0].Sequence.Length);

                sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", t2c.Name, t2c.Seqname, t2c.Start, t2c.End, t2c.Strand, finalSeed, offset, finalSeed.Length, Math.Round(coverage));

                var t = target[j];
                sw.WriteLine("\t{0}:{1}-{2}:{3}\t{4}\t{5}\t{6}",
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