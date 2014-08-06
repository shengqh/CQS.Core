using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Vcf
{
  /// <summary>
  /// A VCF item list readed from VCF file
  /// </summary>
  public class VcfItemList
  {
    public List<string> Comments { get; set; }
    public string Header { get; set; }
    public List<VcfItem> Items { get; set; }

    public VcfItemList()
    {
      this.Comments = new List<string>();
      this.Items = new List<VcfItem>();
    }
  }
}
