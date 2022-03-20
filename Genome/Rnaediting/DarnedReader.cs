using System;
using System.Collections.Generic;

namespace CQS.Genome.Rnaediting
{
  public class DarnedReader : AbstractHeaderFile<RnaeditItem>
  {
    protected override Dictionary<string, Action<string, RnaeditItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, RnaeditItem>>();

      result["chrom"] = (m, n) => n.Chrom = m;
      result["coordinate"] = (m, n) => n.Coordinate = long.Parse(m);
      result["strand"] = (m, n) => n.Strand = m[0];
      result["inchr"] = (m, n) => n.NucleotideInChromosome = m[0];
      result["inrna"] = (m, n) => n.NucleotideInRNA = m[0];
      result["gene"] = (m, n) => n.Gene = m;
      result["seqReg"] = (m, n) => n.SeqReg = m[0];
      result["exReg"] = (m, n) => n.ExReg = string.IsNullOrEmpty(m) ? ' ' : m[0];
      result["source"] = (m, n) => n.Source = m;
      result["PubMed ID"] = (m, n) => n.PubmedId = m;

      return result;
    }
  }
}
