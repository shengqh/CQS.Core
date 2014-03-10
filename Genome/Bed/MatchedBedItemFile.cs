using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.Bed
{
  public class MatchedBedItemFile : LineFile
  {
    public MatchedBedItemFile() : base() { }

    public MatchedBedItemFile(string filename) : base(filename) { }

    private MatchExon ParseMatchExon(string[] parts)
    {
      MatchExon mx = new MatchExon();
      mx.TranscriptId = parts[7];
      mx.TranscriptType = parts[8];
      mx.TranscriptCount = int.Parse(parts[9]);
      mx.RetainedIntron = parts[10] == "1";
      mx.IntronSize = int.Parse(parts[11]);

      var exons = parts[13].Split(';');
      foreach (var exon in exons)
      {
        mx.Add(new Location(exon));
      }
      return mx;
    }

    public MatchedBedItem Next()
    {
      string line = "";
      while ((line = reader.ReadLine()) != null)
      {
        if (line.Length > 0 && line[0] != '\t')
        {
          var parts = line.Split('\t');
          if (parts.Length < 14)
          {
            continue;
          }

          long chromstart;
          if (!long.TryParse(parts[1], out chromstart))
          {
            continue;
          }

          var result = new MatchedBedItem();

          result.Seqname = parts[0];
          result.Start = chromstart;
          result.End = long.Parse(parts[2]);
          result.Name = parts[3];
          result.Score = double.Parse(parts[4]);
          result.Strand = parts[5][0];
          result.Exons.Add(ParseMatchExon(parts));

          int c;
          while ((c = reader.Peek()) != -1)
          {
            if (c != '\t')
            {
              break;
            }

            line = reader.ReadLine();
            parts = line.Split('\t');

            result.Exons.Add(ParseMatchExon(parts));
          }

          return result;
        }
      }

      return null;
    }

    private static BedItemFile<BedItem> bedFile = new BedItemFile<BedItem>();
    private static string missHeader = string.Join("", Enumerable.Repeat('\t', bedFile.HeaderColumnCount - 1));

    public string GetHeader()
    {
      return bedFile.GetHeader() + "\tmatch_count\ttranscriptid\ttranscript_type\ttranscript_count\tintron_retained\tintron_size\texon_count\texons";
    }

    public string GetValue(MatchedBedItem item)
    {
      StringBuilder sb = new StringBuilder();

      bool bFirst = true;
      foreach (var exon in item.Exons)
      {
        if (bFirst)
        {
          sb.Append(string.Format("{0}\t{1}", bedFile.GetValue(item), item.Exons.Count));
        }
        else
        {
          sb.Append(string.Format("{0}\t{1}", missHeader, item.Exons.Count));
        }

        sb.AppendLine(string.Format("\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
          exon.TranscriptId,
          exon.TranscriptType,
          exon.TranscriptCount,
          exon.RetainedIntron ? 1 : 0,
          exon.IntronSize,
          exon.Count,
          string.Join(";", exon.ConvertAll(m => string.Format("{0}-{1}", m.Start, m.End)))));
      }

      return sb.ToString();
    }

    public void WriteToFile(string filename, List<MatchedBedItem> items, bool outputFasta)
    {
      Func<string> headerFunc;
      Func<MatchedBedItem, string> valueFunc;

      var isUnmatchedData = items.All(m => m.Exons.Count == 0);
      if (isUnmatchedData)
      {
        headerFunc = () => bedFile.GetHeader();
        valueFunc = m => bedFile.GetValue(m);
      }
      else
      {
        headerFunc = () => GetHeader();
        valueFunc = m => GetValue(m);
      }

      using (StreamWriter sw = new StreamWriter(filename))
      {
        sw.WriteLine(headerFunc());
        items.ForEach(m => sw.Write(valueFunc(m)));
      }

      if (outputFasta)
      {
        using (StreamWriter sw = new StreamWriter(filename + "_direct.fa"))
        {
          items.ForEach(m =>
          {
            sw.WriteLine(">" + m.Name);
            sw.WriteLine(m.DirectExpectSequence);
          });
        }

        if (!isUnmatchedData)
        {
          using (StreamWriter sw = new StreamWriter(filename + ".fa"))
          {
            items.ForEach(m =>
            {
              if (m.Exons.Count > 0)
              {
                sw.WriteLine(">" + m.Name);
                sw.WriteLine(m.Exons[0].Sequence);
              }
            });
          }
        }
      }
    }
  }
}
