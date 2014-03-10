using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.ChipSeq
{
  public class OverlappedChipSeqComparisonItemFormat : IFileWriter<List<OverlappedChipSeqComparisonItem>>
  {
    public void WriteToFile(string fileName, List<OverlappedChipSeqComparisonItem> t)
    {
      var keys = OverlappedChipSeqComparisonItem.GetKeys(t);

      Dictionary<string, OverlappedChipSeqItemFormat> formats = new Dictionary<string, OverlappedChipSeqItemFormat>();
      foreach (var key in keys)
      {
        formats[key] = new OverlappedChipSeqItemFormat(string.IsNullOrEmpty(key) ? "" : key + "_");
      }

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.Write("Gene Symbol\tChromosome");
        foreach (var key in keys)
        {
          var format = formats[key];
          sw.Write(format.GetDetailHeader());
        }
        sw.WriteLine();

        foreach (var oc in t)
        {
          sw.Write("{0}\t{1}", oc.GeneSymbol, oc.Chromosome);
          foreach (var key in keys)
          {
            var format = formats[key];
            if (oc.ItemMap.ContainsKey(key))
            {
              sw.Write(format.GetDetail(oc.ItemMap[key]));
            }
            else
            {
              sw.Write(format.GetDetail(null));
            }
          }
          sw.WriteLine();
        }
      }
    }
  }
}
