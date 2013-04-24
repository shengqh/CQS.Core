using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarExporter : IAnnotationCsvExporter
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
      this.annovarMap = annovarItems.ToDictionary(m => keyFunc(m.Chrom, m.ChromStart));
      this.emptyStr = new String(',', annovarItems.SummaryHeaderInCsvFormat.Count(m => m == ','));
      this.annovarHeader = (from h in annovarItems.SummaryHeaderInCsvFormat.Split(',')
                            select "annovar_" + h).Merge(',');
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
        return annovarMap[key].SummaryInCsvFormat;
      }
      else
      {
        return emptyStr;
      }
    }
  }
}
