using System.Collections.Generic;

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
