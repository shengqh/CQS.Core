using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.TCGA
{
  public class BarInfo
  {
    public BarInfo(string barcode, string filename, string platform)
    {
      this.BarCode = barcode;
      this.Paticipant = GetPaticipant(barcode);
      this.Platform = platform;
      this.Sample = GetSample(barcode);
      this.FileName = filename;
    }

    public BarInfo(string barcode, string filename)
    {
      this.BarCode = barcode;
      this.Paticipant = GetPaticipant(barcode);
      this.Platform = string.Empty;
      this.Sample = GetSample(barcode);
      this.FileName = filename;
    }

    /// <summary>
    /// TCGA barcode
    /// </summary>
    public string BarCode { get; set; }

    /// <summary>
    /// Paticipant ID
    /// </summary>
    public string Paticipant { get; set; }

    /// <summary>
    /// Sample type, such as primary tumor or normal tissue
    /// </summary>
    public int Sample { get; set; }

    /// <summary>
    /// Location of data file
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The platform of the data file generated from
    /// </summary>
    public string Platform { get; set; }

    /// <summary>
    /// Get paticipant from bar code
    /// </summary>
    /// <param name="barcode">barcode</param>
    /// <returns>paticipant id</returns>
    public static string GetPaticipant(string barcode)
    {
      return barcode.Substring(0, 12);
    }

    /// <summary>
    /// Get sample type id from barcode
    /// </summary>
    /// <param name="barcode">bar code</param>
    /// <returns>sample type id</returns>
    public static int GetSample(string barcode)
    {
      return int.Parse(barcode.Substring(13, 2));
    }
  }
}
