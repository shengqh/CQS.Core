using System.Collections.Generic;

namespace CQS.Genome.Sam
{
  public class SAMItemList1 : List<SAMItem>
  {
    public string QueryName { get; set; }

    public double Score { get; set; }

    public int QueryLength { get; set; }
  }
}
