using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.TCGA
{
  public class BarInfo
  {
    public BarInfo(string barcode, string filename)
    {
      this.BarCode = barcode;
      this.Paticipant = GetPaticipant(barcode);
      this.Sample = GetSample(barcode);
      this.FileName = filename;
    }

    public string BarCode { get; set; }
    public string Paticipant { get; set; }
    public int Sample { get; set; }
    public string FileName { get; set; }

    public static string GetPaticipant(string barcode)
    {
      return barcode.Substring(0, 12);
    }

    public static int GetSample(string barcode)
    {
      return int.Parse(barcode.Substring(13, 2));
    }
  }
}
