using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Pileup
{
  public class EventCount
  {
    public EventCount(string name, int count)
    {
      this.Event = name;
      this.Count = count;
    }
    public string Event { get; set; }
    public int Count { get; set; }
  }
}
