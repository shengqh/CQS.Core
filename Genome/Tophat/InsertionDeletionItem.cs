using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;

namespace CQS.Genome.Tophat
{
  public class InsertionDeletionItem : BedItem
  {
    public long Distance { get; set; }
  }
}
