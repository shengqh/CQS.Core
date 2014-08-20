using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarSummaryItemList : List<AnnovarSummaryItem>
  {
    public AnnovarSummaryItemList()
    {
      this.Headers = new List<string>();
    }

    public List<string> Headers { get; set; }
  }
}
