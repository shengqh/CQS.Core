using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome
{
  public class SingleNucleotidePolymorphism
  {
    public char RefAllele { get; set; }

    public char SampleAllele { get; set; }

    public SingleNucleotidePolymorphism(char refAllele, char sampleAllele)
    {
      this.RefAllele = refAllele;
      this.SampleAllele = sampleAllele;
    }

    public bool IsMutation(char from, char to)
    {
      return RefAllele == from && SampleAllele == to;
    }
  }
}
