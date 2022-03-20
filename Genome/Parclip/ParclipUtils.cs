using CQS.Genome.Bed;
using CQS.Genome.Feature;
using RCPA;
using RCPA.Seq;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Parclip
{
  public static class ParclipUtils
  {
    public const int DEFAULT_COVERAGE = 1000;

    public static List<CoverageRegion> GetSmallRNACoverageRegion(string featureFile, string[] includeSmallRNATags = null, string[] excudeSmallRNATags = null)
    {
      if (featureFile.ToLower().EndsWith(".fasta") || featureFile.ToLower().EndsWith(".fa"))
      {
        return GetSmallRNACoverageRegionFromFasta(featureFile);
      }
      else
      {
        return GetSmallRNACoverageRegionFromXml(featureFile, includeSmallRNATags, excudeSmallRNATags);
      }
    }

    public static List<CoverageRegion> GetSmallRNACoverageRegionFromFasta(string featureFile)
    {
      var sequences = SequenceUtils.Read(featureFile);
      var result = new List<CoverageRegion>();
      foreach (var smallRNA in sequences)
      {
        //coverage in all position will be set as same as total query count
        var rg = new CoverageRegion();
        rg.Name = smallRNA.Name;
        rg.Seqname = "Unknown";
        rg.Start = -1;
        rg.End = -1;
        rg.Strand = '*';
        rg.Sequence = smallRNA.SeqString;

        for (int i = 0; i < smallRNA.SeqString.Length; i++)
        {
          rg.Coverages.Add(new CoverageSite(DEFAULT_COVERAGE));
        }
        result.Add(rg);
      }
      return result;
    }

    public static List<CoverageRegion> GetSmallRNACoverageRegionFromXml(string featureFile, string[] includeSmallRNATags = null, string[] excudeSmallRNATags = null)
    {
      var smallRNAGroups = new FeatureItemGroupXmlFormat().ReadFromFile(featureFile);

      if (includeSmallRNATags != null && includeSmallRNATags.Length > 0)
      {
        smallRNAGroups.ForEach(m => m.RemoveAll(l => includeSmallRNATags.All(k => !m.Name.StartsWith(k))));
        smallRNAGroups.RemoveAll(m => m.Count == 0);
      }

      if (excudeSmallRNATags != null && excudeSmallRNATags.Length > 0)
      {
        smallRNAGroups.ForEach(m => m.RemoveAll(l => excudeSmallRNATags.Any(k => m.Name.StartsWith(k))));
        smallRNAGroups.RemoveAll(m => m.Count == 0);
      }

      var result = new List<CoverageRegion>();
      foreach (var sg in smallRNAGroups)
      {
        //since the items in same group shared same reads, only the first one will be used.
        var smallRNA = sg[0];
        smallRNA.Name = (from g in sg select g.Name).Merge("/");

        smallRNA.Locations.RemoveAll(m => m.SamLocations.Count == 0);
        smallRNA.CombineLocationByMappedReads();

        //only first location will be used.
        var loc = smallRNA.Locations[0];

        //coverage in all position will be set as same as total query count
        var rg = new CoverageRegion();
        rg.Name = smallRNA.Name;
        rg.Seqname = loc.Seqname;
        rg.Start = loc.Start;
        rg.End = loc.End;
        rg.Strand = loc.Strand;
        rg.Sequence = loc.Sequence;

        var coverage = (from sloc in loc.SamLocations select sloc.SamLocation.Parent.QueryCount).Sum();
        var uniqueRead = (from sloc in loc.SamLocations select sloc.SamLocation.Parent.Qname).Distinct().ToList();

        for (int i = 0; i < loc.Length; i++)
        {
          rg.Coverages.Add(new CoverageSite(coverage, uniqueRead));
        }
        result.Add(rg);
      }
      return result;
    }

    public static SeedItem GetSeed(CoverageRegion cr, int offset, int seedLength, double minCoverage)
    {
      if (cr.Sequence.Length < offset + seedLength)
      {
        return null;
      }

      var coverages = cr.Coverages.Skip(offset).Take(seedLength).ToList();
      var coverage = coverages.Average(l => l.Coverage);
      if (coverage < minCoverage)
      {
        return null;
      }

      var newseq = cr.Sequence.Substring(offset, seedLength);
      var start = cr.Start + offset;
      var end = cr.Start + offset + seedLength - 1;
      if (cr.Strand == '+')
      {
        newseq = SequenceUtils.GetReverseComplementedSequence(newseq);
      }

      return new SeedItem()
      {
        Seqname = cr.Seqname,
        Start = start,
        End = end,
        Strand = cr.Strand,
        Coverage = coverage,
        Name = cr.Name,
        Sequence = newseq,
        Source = cr,
        SourceOffset = offset,
        GeneSymbol = cr.GeneSymbol
      };
    }

    public static List<CoverageRegion> GetTargetCoverageRegion(ITargetBuilderOptions options, IProgressCallback progress, bool removeRegionWithoutSequence = true)
    {
      List<CoverageRegion> result;
      if (options.TargetFile.EndsWith(".xml"))
      {
        result = GetTargetCoverageRegionFromXml(options, progress);
      }
      else
      {
        result = GetTargetCoverageRegionFromBed(options, progress);
      }

      var dic = result.ToGroupDictionary(m => m.Seqname);

      progress.SetMessage("Filling sequence from {0}...", options.GenomeFastaFile);
      using (var sr = new StreamReader(options.GenomeFastaFile))
      {
        var ff = new FastaFormat();
        Sequence seq;
        while ((seq = ff.ReadSequence(sr)) != null)
        {
          progress.SetMessage("Processing chromosome {0} ...", seq.Reference);
          var seqname = seq.Name.StringAfter("chr");
          List<CoverageRegion> lst;
          if (dic.TryGetValue(seqname, out lst))
          {
            foreach (var l in lst)
            {
              l.Sequence = seq.SeqString.Substring((int)(l.Start - 1), (int)l.Length);
              if (l.Strand == '+')
              {
                l.ReverseComplementedSequence = SequenceUtils.GetReverseComplementedSequence(l.Sequence);
              }
            }
          }
        }
      }
      if (removeRegionWithoutSequence)
      {
        result.RemoveAll(l => string.IsNullOrEmpty(l.Sequence));
      }

      progress.SetMessage("Filling sequence finished.");

      var namemap = new MapReader(1, 12).ReadFromFile(options.RefgeneFile);
      result.ForEach(m =>
      {
        var gene = m.Name.StringBefore("_utr3");
        m.GeneSymbol = namemap.ContainsKey(gene) ? namemap[gene] : string.Empty;
      });

      return result;
    }

    public static List<CoverageRegion> GetTargetCoverageRegionFromXml(ITargetBuilderOptions options, IProgressCallback progress)
    {
      var result = new List<CoverageRegion>();

      var groups = new FeatureItemGroupXmlFormat().ReadFromFile(options.TargetFile);
      progress.SetMessage("Total {0} potential target group read from file {1}", groups.Count, options.TargetFile);

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
          var map = new Dictionary<long, CoverageSite>();
          foreach (var sloc in loc.SamLocations)
          {
            for (long i = sloc.SamLocation.Start; i <= sloc.SamLocation.End; i++)
            {
              CoverageSite count;
              if (map.TryGetValue(i, out count))
              {
                count.Coverage = count.Coverage + sloc.SamLocation.Parent.QueryCount;
                count.UniqueRead.Add(sloc.SamLocation.Parent.Qname);
              }
              else
              {
                map[i] = new CoverageSite(sloc.SamLocation.Parent.QueryCount, sloc.SamLocation.Parent.Qname);
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

      return result;
    }

    /// <summary>
    /// Transfer bed format (zero-based) to gff format (one-based)
    /// </summary>
    /// <param name="options"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static List<CoverageRegion> GetTargetCoverageRegionFromBed(ITargetBuilderOptions options, IProgressCallback progress)
    {
      var result = new List<CoverageRegion>();

      var groups = new BedItemFile<BedItem>().ReadFromFile(options.TargetFile);
      progress.SetMessage("Total {0} potential target group read from file {1}", groups.Count, options.TargetFile);

      foreach (var utr in groups)
      {
        var rg = new CoverageRegion();
        rg.Name = utr.Name;
        rg.Seqname = utr.Seqname.StringAfter("chr");
        rg.Start = utr.Start + 1;
        rg.End = utr.End;
        rg.Strand = utr.Strand;
        for (var i = rg.Start; i < rg.End; i++)
        {
          rg.Coverages.Add(new CoverageSite(DEFAULT_COVERAGE));
        }
        result.Add(rg);
      }

      return result;
    }


    public static List<SeedItem> BuildTargetSeeds(ITargetBuilderOptions options, List<string> seeds, IProgressCallback progress)
    {
      List<SeedItem> result = new List<SeedItem>();

      var mapped = GetTargetCoverageRegion(options, progress);

      progress.SetMessage("Building seeds ...");
      progress.SetRange(0, mapped.Count);
      progress.SetPosition(0);
      foreach (var l in mapped)
      {
        progress.Increment(1);
        foreach (var seed in seeds)
        {
          var curseq = l.Strand == '+' ? l.ReverseComplementedSequence : l.Sequence;
          int lastpos = -1;
          while (true)
          {
            int pos = curseq.IndexOf(seed, lastpos + 1);
            if (pos == -1)
            {
              break;
            }

            if (l.Strand == '+')
            {
              result.Add(GetSeed(l, curseq.Length - pos - options.MinimumSeedLength, options.MinimumSeedLength, options.MinimumCoverage));
            }
            else
            {
              result.Add(GetSeed(l, pos, options.MinimumSeedLength, options.MinimumCoverage));
            }
            lastpos = pos;
          }
        }
      }
      progress.End();
      progress.SetMessage("Total {0} {1}mers seeds were built.", result.Count, options.MinimumSeedLength);

      return result;
    }

    public static List<SeedItem> BuildTargetSeeds(ITargetBuilderOptions options, Func<SeedItem, bool> acceptSeed, IProgressCallback progress)
    {
      List<SeedItem> seeds = new List<SeedItem>();

      var mapped = GetTargetCoverageRegion(options, progress);

      progress.SetMessage("Building seeds ...");
      progress.SetRange(0, mapped.Count);
      progress.SetPosition(0);
      foreach (var l in mapped)
      {
        progress.Increment(1);
        for (int i = 0; i < l.Sequence.Length - options.MinimumSeedLength; i++)
        {
          SeedItem si = GetSeed(l, i, options.MinimumSeedLength, options.MinimumCoverage);

          if (si != null && acceptSeed(si))
          {
            seeds.Add(si);
          }
        }
      }
      progress.End();
      progress.SetMessage("Total {0} {1}mers seeds were built.", seeds.Count, options.MinimumSeedLength);

      return seeds;
    }

    public static Dictionary<string, List<SeedItem>> BuildTargetSeedMap(ITargetBuilderOptions options, List<string> seeds, IProgressCallback progress)
    {
      //Read 6 mers from target
      var targetSeeds = BuildTargetSeeds(options, seeds, progress);
      progress.SetMessage("Grouping seeds by sequence ...");
      var result = targetSeeds.ToGroupDictionary(m => m.Sequence.ToUpper());
      progress.SetMessage("Total {0} unique {1}mers seeds were built.", result.Count, options.MinimumSeedLength);
      return result;
    }

    public static Dictionary<string, List<SeedItem>> BuildTargetSeedMap(ITargetBuilderOptions options, Func<SeedItem, bool> acceptSeed, IProgressCallback progress)
    {
      //Read 6 mers from target
      var targetSeeds = BuildTargetSeeds(options, acceptSeed, progress);
      progress.SetMessage("Grouping seeds by sequence ...");
      var result = targetSeeds.ToGroupDictionary(m => m.Sequence.ToUpper());
      progress.SetMessage("Total {0} unique {1}mers seeds were built.", result.Count, options.MinimumSeedLength);
      return result;
    }

    public static List<SeedItem> ExtendToLongestTarget(List<SeedItem> target, CoverageRegion t2c, string seq, int offset, int minimumSeedLength, int maximumSeedLength, double minimumCoverage)
    {
      var individualSeeds = new List<SeedItem>();
      var source = new List<SeedItem>(target);
      var extendSeedLength = minimumSeedLength;
      while (extendSeedLength < maximumSeedLength)
      {
        extendSeedLength++;

        //check the coverage in smallRNA
        if (t2c != null)
        {
          var extendCoverage = t2c.Coverages.Skip(offset).Take(extendSeedLength).Average(l => l.Coverage);
          if (extendCoverage < minimumCoverage)
          {
            break;
          }
        }

        if (seq.Length < offset + extendSeedLength)
        {
          individualSeeds.AddRange(source);
          break;
        }

        var extendSeed = seq.Substring(offset, extendSeedLength);

        var extendTarget = new List<SeedItem>();
        var doneTarget = new List<SeedItem>();
        foreach (var utrTarget in source)
        {
          var newoffset = utrTarget.Strand == '-' ? utrTarget.SourceOffset : utrTarget.SourceOffset - 1;
          if (newoffset < 0)
          {
            doneTarget.Add(utrTarget);
            continue;
          }

          var newseed = GetSeed(utrTarget.Source, newoffset, extendSeedLength, minimumCoverage);
          if (newseed == null)
          {
            doneTarget.Add(utrTarget);
            continue;
          }

          if (!extendSeed.Equals(newseed.Sequence))
          {
            doneTarget.Add(utrTarget);
            continue;
          }

          extendTarget.Add(newseed);
        }

        individualSeeds.AddRange(doneTarget);

        if (extendTarget.Count > 0)
        {
          source = extendTarget;
        }
        else
        {
          break;
        }
      }

      //For each gene, only the longest one will be kept
      var individualGenes = (from cs in individualSeeds.GroupBy(m => m.GeneSymbol)
                             let csitem = cs.GroupBy(l => l.Length).OrderByDescending(k => k.Key).First()
                             from ls in csitem
                             select ls).OrderByDescending(l => l.Length).ToList();

      //Merge the seeds with same gene symbol, same location but different name
      var final = new List<SeedItem>();
      var rmap = individualGenes.GroupBy(l => l.GeneSymbol + "_" + l.GetLocation()).ToList();
      foreach (var rm in rmap)
      {
        var item = rm.First();
        item.Name = (from r in rm
                     select r.Name).Merge("/");
        final.Add(item);
      }

      return final.OrderByDescending(l => l.Length).ToList();
    }

    public static List<SeedItem> FindLongestTarget(List<SeedItem> target, CoverageRegion t2c, string seq, int offset, int minimumSeedLength, int maximumSeedLength, double minimumCoverage)
    {
      var result = ExtendToLongestTarget(target, t2c, seq, offset, minimumSeedLength, maximumSeedLength, minimumCoverage);
      var maxLength = result.Max(l => l.Length);
      result.RemoveAll(l => l.Length < maxLength);
      return result;
    }
  }
}
