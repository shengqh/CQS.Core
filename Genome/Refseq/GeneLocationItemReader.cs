using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.Refseq
{
  public class GeneLocationItemReader:IFileReader<List<GeneLocationItem>>
  {
    private Regex reg = new Regex("gene_id\\s\"(\\S+?)\"; transcript_id\\s\"(\\S+?)\"");

    private Func<GeneLocationItem, bool> filter;
    public GeneLocationItemReader(Func<GeneLocationItem, bool> filter=null)
    {
      this.filter = filter;
    }

    public List<GeneLocationItem> ReadFromFile(string fileName)
    {
      var result = new List<GeneLocationItem>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (parts.Length >= 9)
          {
            var item = new GeneLocationItem();

            item.Chromosome = parts[0];
            item.Database = parts[1];
            item.ItemType = parts[2];
            item.Start = long.Parse(parts[3]);
            item.End = long.Parse(parts[4]);
            item.Unknown1 = double.Parse(parts[5]);
            item.Strand = parts[6][0];
            item.Unknown2 = parts[7][0];

            var m = reg.Match(parts[8]);
            if (m.Success)
            {
              item.GeneId = m.Groups[1].Value;
              item.TranscriptId = m.Groups[2].Value;
            }
            else
            {
              throw new Exception("Cannot get geneid or transcriptionid from " + parts[8] + "\n" + line);
            }

            if ((null == filter) || filter(item))
            {
              result.Add(item);
            }
          }
        }
      }

      return result;
    }
  }
}
