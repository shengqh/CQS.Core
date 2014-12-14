using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Feature
{
  public class FeatureSamLocation
  {
    public FeatureSamLocation(FeatureLocation featureLocation)
    {
      this.FeatureLocation = featureLocation;
      this.FeatureLocation.SamLocations.Add(this);
    }

    private SamAlignedLocation _samLocation;

    public SamAlignedLocation SamLocation
    {
      get
      {
        return _samLocation;
      }
      set
      {
        _samLocation = value;

        CalculateOffset();

        if (value != null && !value.Features.Contains(this.FeatureLocation))
        {
          value.Features.Add(this.FeatureLocation);
        }
      }
    }

    public FeatureLocation FeatureLocation { get; private set; }

    public long Offset { get; set; }

    public double OverlapPercentage { get; set; }

    private void CalculateOffset()
    {
      if (FeatureLocation == null || _samLocation == null)
      {
        this.Offset = int.MaxValue;
        return;
      }

      if (FeatureLocation.Strand == '-')
      {
        this.Offset = FeatureLocation.End - _samLocation.End;
      }
      else
      {
        //default, assume as positive strand
        this.Offset = _samLocation.Start - FeatureLocation.Start;
      }
    }
  }
}
