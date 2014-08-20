using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarSummaryItemListReader:IFileReader<AnnovarSummaryItemList>
  {
    public AnnovarSummaryItemList ReadFromFile(string fileName)
    {
      var result = new AnnovarSummaryItemList();

      using (var sr = new StreamReader(fileName))
      {
        var header = sr.ReadLine().Split('\t');
        result.Headers = header.Skip(5).ToList();
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (parts.Length < 5)
          {
            continue;
          }

          var item = new AnnovarSummaryItem();
          item.Seqname = parts[0];
          item.Start = long.Parse(parts[1]);
          item.End = long.Parse(parts[2]);
          item.RefAllele = parts[3];
          item.AltAllele = parts[4];
          item.Values = parts.Skip(5).ToList();
          result.Add(item);
        }
      }

      return result;
    }
  }
}
