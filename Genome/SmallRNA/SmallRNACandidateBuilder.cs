using CQS.Genome.Mapping;
using CQS.Genome.Sam;
using System;
using System.Collections.Generic;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACandidateBuilder : SAMAlignedItemCandidateBuilder
  {
    private SmallRNACountProcessorOptions options;
    private HashSet<string>  rangeQueries;

    public SmallRNACandidateBuilder(SmallRNACountProcessorOptions options) : base(options)
    {
      this.options = options;
    }

    protected override bool AcceptQueryName(string qname)
    {
      return rangeQueries.Contains(qname);
    }

    protected override List<T> DoBuild<T>(string fileName, out HashSet<string> totalQueryNames)
    {
      Progress.SetMessage("Find queries overlapped with coordinates...");
      rangeQueries = new HashSet<string>();
      using (var sr = SAMFactory.GetReader(fileName, true, options.CoordinateFile))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var qname = line.StringBefore("\t");
          rangeQueries.Add(qname);
        }
      }
      Progress.SetMessage("{0} queries overlaped with coordinates.", rangeQueries.Count);

      return base.DoBuild<T>(fileName, out totalQueryNames);
    }
  }
}
