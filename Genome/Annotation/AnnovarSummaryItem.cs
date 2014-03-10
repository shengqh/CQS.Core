using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.Annotation
{
  public class AnnovarSummaryItem : SequenceRegion
  {
    public string SummaryInCsvFormat { get; set; }
  }

  public class AnnovarSummaryItemList : List<AnnovarSummaryItem>
  {
    public string SummaryHeaderInCsvFormat { get; set; }
  }

  public class AnnovarSummaryItemListReader : IFileReader<AnnovarSummaryItemList>
  {
    private Regex head = new Regex("(.+),Chr,Start,End,Ref,Obs,Otherinfo");
    
    private Regex value = new Regex(@"(.+),([^,]+),(\d+),(\d+),[^,]*,[^,]*,$");

    public AnnovarSummaryItemList ReadFromFile(string fileName)
    {
      var result = new AnnovarSummaryItemList();
      using (StreamReader sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var match = head.Match(line);
        result.SummaryHeaderInCsvFormat = match.Groups[1].Value;

        while ((line = sr.ReadLine()) != null)
        {
          var v = value.Match(line);
          if (v.Success)
          {
            var item = new AnnovarSummaryItem();
            item.SummaryInCsvFormat = v.Groups[1].Value;
            item.Seqname = v.Groups[2].Value;
            item.Start = long.Parse(v.Groups[3].Value);
            item.End = long.Parse(v.Groups[4].Value);
            result.Add(item);
          }
        }
      }

      return result;
    }
  }
}
