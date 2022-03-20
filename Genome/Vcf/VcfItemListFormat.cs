using RCPA;
using System.IO;

namespace CQS.Genome.Vcf
{
  public class VcfItemListFormat : IFileFormat<VcfItemList>
  {
    public VcfItemList ReadFromFile(string fileName)
    {
      var result = new VcfItemList();
      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("##"))
          {
            result.Comments.Add(line);
            continue;
          }

          if (line.StartsWith("#"))
          {
            result.Header = line;
            break;
          }
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (string.IsNullOrEmpty(line.Trim()))
          {
            continue;
          }

          var parts = line.Split('\t');
          var item = new VcfItem()
          {
            Seqname = parts[0],
            Start = long.Parse(parts[1]),
            End = long.Parse(parts[2]),
            RefAllele = parts[3],
            AltAllele = parts[4],
            Line = line
          };

          result.Items.Add(item);
        }
      }
      return result;
    }

    public void WriteToFile(string fileName, VcfItemList t)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        t.Comments.ForEach(m => sw.WriteLine(m));
        sw.WriteLine(t.Header);
        t.Items.ForEach(m => sw.WriteLine(m.Line));
      }
    }
  }
}
