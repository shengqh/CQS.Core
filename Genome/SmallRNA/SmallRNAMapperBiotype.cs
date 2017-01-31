using CQS.Genome.Feature;
using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAMapperBiotype : SmallRNAMapper
  {
    public SmallRNAMapperBiotype(SmallRNABiotype biotype, ISmallRNACountProcessorOptions options) : base(biotype.ToString(), options, feature => feature.Category.Equals(biotype.ToString())) { }
  }
}
