using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.QC
{
  public class SamToolsStatItem
  {
    public int Total { get; set; }
    public int Duplicates { get; set; }
    public int Mapped { get; set; }
    public int PairedInSequencing { get; set; }
    public int Read1 { get; set; }
    public int Read2 { get; set; }
    public int ProperlyPaired { get; set; }
    public int WithItselfAndMateMapped { get; set; }
    public int Singletons { get; set; }
    public int WithMateMappedToADifferentChr { get; set; }
    public int WithMateMappedToADifferentChrMapQ { get; set; }
  }
}
