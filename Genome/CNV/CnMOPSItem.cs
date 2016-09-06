using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.CNV
{
  public class CnMOPsItem : CNVItem
  {
    public double Median { get; set; }
    public double Mean { get; set; }
    public string CN { get; set; }
  }

  public class CnMOPsItemReader : CNVItemReader<CnMOPsItem>
  {
    protected override Dictionary<string, Action<string, CnMOPsItem>> GetHeaderActionMap()
    {
      var result = base.GetHeaderActionMap();

      result["seqnames"] = CNVItemUtils.FuncChrom;
      result["sampleName"] = CNVItemUtils.FuncFileName;
      result["CN"] = (m, n) => n.CN = m;

      return result;
    }
  }
}
