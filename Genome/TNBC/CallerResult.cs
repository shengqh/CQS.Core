using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.TNBC
{
  public enum TNBCSubtype { UNS, IM, BL1, MSL, M, BL2, LAR };

  public class CallerResultValue
  {
    public double Coef { get; set; }
    public double Pvalue { get; set; }
  }

  public class CallerResult : RCPA.Annotation
  {
    public string Sample { get; set; }

    public string SampleType { get; set; }

    public string Dataset { get; set; }

    public string Patient { get; set; }

    public string Geneset { get; set; }

    public Dictionary<TNBCSubtype, CallerResultValue> Items { get; private set; }

    public CallerResult()
    {
      this.Dataset = string.Empty;
      this.SampleType = string.Empty;
      this.Patient = string.Empty;
      this.Items = new Dictionary<TNBCSubtype, CallerResultValue>();
    }

    public KeyValuePair<TNBCSubtype, CallerResultValue> GetSubtype()
    {
      var valid = this.Items.Where(l => !l.Key.Equals(TNBCSubtype.IM) && !l.Key.Equals(TNBCSubtype.MSL)).ToList();
      var max = valid.Max(l => l.Value.Coef);
      return valid.Where(l => l.Value.Coef == max).First();
    }
  }
}
