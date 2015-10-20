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
  public class ParclipMiRNATargetBuilder : AbstractTargetBuilder
  {
    private ParclipMiRNATargetBuilderOptions options;

    public ParclipMiRNATargetBuilder(ParclipMiRNATargetBuilderOptions options)
      : base(options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var namemap = new MapReader(1, 12).ReadFromFile(options.RefgeneFile);

      Progress.SetMessage("Build target {0} mers...", options.SeedLength);

      //Read 6 mers from target
      var utr3seeds = BuildTargetSeeds(options.SeedLength);
      utr3seeds.ForEach(m =>
      {
        var gene = m.Name.StringBefore("_utr3");
        m.GeneSymbol = namemap.ContainsKey(gene) ? namemap[gene] : string.Empty;
      });

      var sixmerMap = utr3seeds.ToGroupDictionary(m => m.Sequence.ToUpper());

      Console.WriteLine("Reading T2C miRNA...");
      var mappedSmallRNA = GetMiRNACoverageRegion(options.InputFile);
      mappedSmallRNA.Sort((m1, m2) => m2.Coverages.Average().CompareTo(m1.Coverages.Average()));

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("miRNA\tChr\tStart\tEnd\tStrand\tSeed\tSeedOffset\tSeedCoverage\tTarget\tTargetCoverage\tTargetGeneSymbol\tTargetName");
        List<SeedItem> target;

        foreach (var t2c in mappedSmallRNA)
        {
          var seq = t2c.Sequence.ToUpper();

          int offset = 1;
          var seed = seq.Substring(offset, options.SeedLength);
          var coverage = t2c.Coverages.Skip(offset).Take(options.SeedLength).Average();
          if (coverage < options.MinimumCoverage)
          {
            continue;
          }

          if (sixmerMap.TryGetValue(seed, out target))
          {
            target.Sort((m1, m2) =>
            {
              return m2.Coverage.CompareTo(m1.Coverage);
            });

            for (int j = 0; j < target.Count; j++)
            {
              sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", t2c.Name.StringAfter(SmallRNAConsts.miRNA + ":"), t2c.Seqname, t2c.Start, t2c.End, t2c.Strand, seed, offset, Math.Round(coverage));

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

      return new[] { options.OutputFile };
    }

    private static List<CoverageRegion> GetMiRNACoverageRegion(string mappedFeatureXmlFile)
    {
      var result = new List<CoverageRegion>();

      var miRNAGroups = new FeatureItemGroupXmlFormat().ReadFromFile(mappedFeatureXmlFile).Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).ToList();
      foreach (var miRNAGroup in miRNAGroups)
      {
        //since the items in same group shared same reads, only the first one will be used.
        var miRNA = miRNAGroup[0];
        miRNA.Name = (from g in miRNAGroup select g.Name).Merge("/");

        miRNA.Locations.RemoveAll(m => m.SamLocations.Count == 0);
        miRNA.CombineLocationByMappedReads();

        //only first location will be used.
        var loc = miRNA.Locations[0];

        //coverage in all position will be set as same as total query count
        var rg = new CoverageRegion();
        rg.Name = miRNA.Name;
        rg.Seqname = loc.Seqname;
        rg.Start = loc.Start;
        rg.End = loc.End;
        rg.Strand = loc.Strand;
        rg.Sequence = loc.Sequence;

        var coverage = (from sloc in loc.SamLocations select sloc.SamLocation.Parent.QueryCount).Sum();

        for (int i = 0; i < loc.Length; i++)
        {
          rg.Coverages.Add(coverage);
        }
        result.Add(rg);
      }
      return result;
    }
  }
}