using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarExporter : IAnnotationTsvExporter
  {
    AnnovarSummaryItemList annovarItems;
    Dictionary<string, AnnovarSummaryItem> annovarMap;
    Func<string, long, string> keyFunc;
    string annovarHeader;
    string emptyStr;

    public AnnovarExporter(string fileName, Func<string, long, string> keyFunc)
    {
      this.keyFunc = keyFunc;
      this.annovarItems = new AnnovarSummaryItemListReader().ReadFromFile(fileName);
      this.annovarMap = annovarItems.ToDictionary(m => keyFunc(m.Seqname, m.Start));
      this.emptyStr = new String('\t', annovarItems.Headers.Count());
      this.annovarHeader = (from h in annovarItems.Headers
                            select "annovar_" + h).Merge('\t');
    }

    public string GetHeader()
    {
      return annovarHeader;
    }

    public string GetValue(string chrom, long start, long end)
    {
      var key = keyFunc(chrom, start);
      if (annovarMap.ContainsKey(key))
      {
        return annovarMap[key].Values.Merge("\t");
      }
      else
      {
        return emptyStr;
      }
    }
  }
}
