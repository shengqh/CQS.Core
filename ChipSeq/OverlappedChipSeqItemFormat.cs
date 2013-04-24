using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.ChipSeq
{
  public class OverlappedChipSeqItemFormat:IFileFormat<List<OverlappedChipSeqItem>>
  {
    public List<OverlappedChipSeqItem> ReadFromFile(string fileName)
    {
      throw new NotImplementedException();
    }

    private string suffix;
    public OverlappedChipSeqItemFormat(string suffix = "")
    {
      this.suffix = suffix;
    }

    public string GetDetailHeader()
    {
      return string.Format("{0}Start\t{0}End\t{0}Length\t{0}Overlap Type\t{0}Total Treatment Count\t{0}Total Control Count\t{0}Ratio\t{0}ExpCount\t{0}Details", this.suffix);
    }

    public string GetDetail(OverlappedChipSeqItem oc)
    {
      if (oc != null)
      {
        return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", oc.Start, oc.End, oc.MappedLength, oc.OverlapType, oc.TreatmentCount, oc.ControlCount, oc.Ratio, oc.FileCount, oc.Details);
      }
      else
      {
        return "\t\t\t\t\t\t\t\t";
      }
    }

    public void WriteToFile(string fileName, List<OverlappedChipSeqItem> t)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Gene Symbol\tChromosome\t" + GetDetailHeader());
        foreach (var oc in t)
        {
          sw.WriteLine("{0}\t{1}\t{2}", oc.GeneSymbol, oc.Chromosome, GetDetail(oc));
        }
      }
    }
  }
}
