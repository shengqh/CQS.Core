using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public class SAMItemList1:List<SAMItem>
  {
    public string QueryName { get; set; }

    public double Score { get; set; }

    public int QueryLength { get; set; }
  }
}
