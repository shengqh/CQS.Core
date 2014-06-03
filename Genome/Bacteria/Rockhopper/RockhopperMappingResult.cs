using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperMappingResult
  {
    public string FileName { get; set; }
    public string TotalReads { get; set; }
    public string AlignedReads { get; set; }
    public string AlignedReadsPercentage { get; set; }
    public string ProteinReadsSense { get; set; }
    public string ProteinReadsAntisense { get; set; }
    public string RibosomalRNAReadsSense { get; set; }
    public string RibosomalRNAReadsAntisense { get; set; }
    public string TransferReadsSense { get; set; }
    public string TransferReadsAntisense { get; set; }
    public string MiscRNAReadsSense { get; set; }
    public string MiscRNAReadsAntisense { get; set; }
    public string UnannotatedRead { get; set; }
  }
}
