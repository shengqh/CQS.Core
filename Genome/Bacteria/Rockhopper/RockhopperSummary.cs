using System.Collections.Generic;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperSummary
  {
    public RockhopperSummary()
    {
      this.MappingResults = new List<RockhopperMappingResult>();
    }

    public string ComparisonName { get; set; }
    public string DifferentialGenes { get; set; }

    public List<RockhopperMappingResult> MappingResults { get; set; }
  }
}
