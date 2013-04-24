using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Dbsnp
{
  public class DbsnpItem
  {
    public DbsnpItem()
    {
    }

    /// <summary>
    /// The name of the chromosome 
    /// </summary>
    public string Chrom { get; set; }

    /// <summary>
    /// Position in sequence (starting from 1)
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// DbSNP identifier
    /// </summary>
    public string Id { get; set; }

    public string Reference { get; set; }

    public string Alternative { get; set; }

    public string Quality { get; set; }

    public string Filter { get; set; }

    public string Information { get; set; }

    public string VariationClass { get; set; }
  }
}
