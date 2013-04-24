using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Tophat
{
  public class JunctionItem
  {
    public string Chr { get; set; }
    public long Start1 { get; set; }
    public long End1 { get; set; }
    public long Start2 { get; set; }
    public long End2 { get; set; }
    public string Name { get; set; }
    public long DistanceJunction { get; set; }
    public long DistanceTerminal { get; set; }
  }
}
