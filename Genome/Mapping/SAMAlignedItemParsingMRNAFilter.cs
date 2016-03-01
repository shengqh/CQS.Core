using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using CQS.Genome.Sam;
using CQS.Genome.SmallRNA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  /// <summary>
  /// For mRNA data, we need to filter by limited coordinates and percentage
  /// </summary>
  public class SAMAlignedItemParsingMRNAFilter : SAMAlignedItemParsingFilter
  {
    delegate bool AcceptQueryNameFunc(string qname);

    private string lastSeqname;

    private List<FeatureLocation> lastFeatures;

    private Dictionary<string, List<FeatureLocation>> featureMap;

    private double minOverlapPercentage;

    private AcceptQueryNameFunc DoAcceptQueryName;

    private static int tagLength = SmallRNAConsts.NTA_TAG.Length;

    private static bool FilterNTA(string qname)
    {
      var pos = qname.IndexOf(SmallRNAConsts.NTA_TAG);
      if (pos == -1)
      {
        return true;
      }
      return pos == qname.Length - tagLength;
    }

    private static bool NotFilterNTA(string qname)
    {
      return true;
    }

    public SAMAlignedItemParsingMRNAFilter(Dictionary<string, List<FeatureLocation>> featureMap, double minOverlapPercentage, bool filterNTA)
    {
      this.featureMap = featureMap;
      this.minOverlapPercentage = minOverlapPercentage;
      if (filterNTA)
      {
        DoAcceptQueryName = FilterNTA;
      }
      else
      {
        DoAcceptQueryName = NotFilterNTA;
      }
    }

    public override bool AcceptQueryName(string qname)
    {
      return DoAcceptQueryName(qname);
    }

    public override bool AcceptLocus(SAMAlignedLocation loc)
    {
      var result = false;
      if (!loc.Seqname.Equals(lastSeqname))
      {
        if (!featureMap.TryGetValue(loc.Seqname, out lastFeatures))
        {
          return false;
        }
        lastSeqname = loc.Seqname;
      }

      foreach (var feature in lastFeatures)
      {
        if (feature.End < loc.Start)
        {
          continue;
        }

        if (feature.Start > loc.End)
        {
          break;
        }

        if (feature.Overlap(loc, this.minOverlapPercentage))
        {
          result = true;
          var samloc = new FeatureSamLocation(feature);
          samloc.SamLocation = loc;
          feature.SamLocations.Add(samloc);
        }
      }

      return result;
    }
  }
}
