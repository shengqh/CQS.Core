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
  public abstract class AbstractTargetBuilder : AbstractThreadProcessor
  {
    private AbstractTargetBuilderOptions options;

    public AbstractTargetBuilder(AbstractTargetBuilderOptions options)
    {
      this.options = options;
    }

    protected class CoverageRegion : SequenceRegion
    {
      public CoverageRegion()
      {
        this.Coverages = new List<int>();
      }

      public List<int> Coverages { get; private set; }
    }

    protected class SeedItem : SequenceRegion
    {
      public double Coverage { get; set; }

      public string GeneSymbol { get; set; }
    }

    protected List<SeedItem> BuildTargetSeeds(int seedLength)
    {
      List<SeedItem> seeds = new List<SeedItem>();

      var mapped = GetTargetCoverageRegion(options.TargetXmlFile);
      foreach (var l in mapped)
      {
        for (int i = 0; i < l.Sequence.Length - seedLength; i++)
        {
          var coverage = l.Coverages.Skip(i).Take(seedLength).Average();
          if (coverage < options.MinimumCoverage)
          {
            continue;
          }

          var newseq = l.Sequence.Substring(i, seedLength);
          var start = l.Start + i;
          var end = l.Start + i + seedLength - 1;
          if (l.Strand == '+')
          {
            newseq = SequenceUtils.GetReverseComplementedSequence(newseq);
          }

          var si = new SeedItem()
          {
            Seqname = l.Seqname,
            Start = start,
            End = end,
            Strand = l.Strand,
            Coverage = coverage,
            Name = l.Name,
            Sequence = newseq
          };

          seeds.Add(si);
        }
      }

      return seeds;
    }

    private List<CoverageRegion> GetTargetCoverageRegion(string targetXmlFile)
    {
      var result = new List<CoverageRegion>();

      var groups = new FeatureItemGroupXmlFormat().ReadFromFile(targetXmlFile);
      foreach (var group in groups)
      {
        //since the items in same group shared same reads, only the first one will be used.
        for (int i = 1; i < group.Count; i++)
        {
          group[0].Name = group[0].Name + "/" + group[i].Name;
        }

        group.RemoveRange(1, group.Count - 1);

        var utr = group[0];

        utr.Locations.RemoveAll(m => m.SamLocations.Count == 0);
        utr.CombineLocationByMappedReads();

        foreach (var loc in utr.Locations)
        {
          var map = new Dictionary<long, int>();
          foreach (var sloc in loc.SamLocations)
          {
            for (long i = sloc.SamLocation.Start; i <= sloc.SamLocation.End; i++)
            {
              int count;
              if (map.TryGetValue(i, out count))
              {
                map[i] = count + sloc.SamLocation.Parent.QueryCount;
              }
              else
              {
                map[i] = sloc.SamLocation.Parent.QueryCount;
              }
            }
          }

          var keys = (from k in map.Keys
                      orderby k
                      select k).ToList();

          int start = 0;
          int end = start + 1;
          while (true)
          {
            if (end == keys.Count || keys[end] != keys[end - 1] + 1)
            {
              var rg = new CoverageRegion();
              rg.Name = utr.Name;
              rg.Seqname = loc.Seqname;
              rg.Start = keys[start];
              rg.End = keys[end - 1];
              rg.Strand = loc.Strand;
              for (int i = start; i < end; i++)
              {
                rg.Coverages.Add(map[keys[i]]);
              }
              result.Add(rg);

              if (end == keys.Count)
              {
                break;
              }

              start = end;
              end = start + 1;
            }
            else
            {
              end++;
            }
          }
        }
      }

      var dic = result.ToGroupDictionary(m => m.Seqname);

      Progress.SetMessage("Filling sequence from {0}...", options.GenomeFastaFile);
      using (var sr = new StreamReader(options.GenomeFastaFile))
      {
        var ff = new FastaFormat();
        Sequence seq;
        while ((seq = ff.ReadSequence(sr)) != null)
        {
          Console.WriteLine(seq.Reference);
          var seqname = seq.Name.StringAfter("chr");
          List<CoverageRegion> lst;
          if (dic.TryGetValue(seqname, out lst))
          {
            foreach (var l in lst)
            {
              l.Sequence = seq.SeqString.Substring((int)(l.Start - 1), (int)l.Length);
            }
          }
        }
      }

      return result;
    }
  }
}