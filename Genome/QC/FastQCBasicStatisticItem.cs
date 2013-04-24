using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.QC
{
  public class FastQCBasicStatisticItem
  {
    public string FastQCVersion { get; set; }
    public bool Passed { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string Encoding { get; set; }
    public long TotalSequences { get; set; }
    public long FilteredSequences { get; set; }
    public int SequenceLength { get; set; }
    public double GC { get; set; }

    public string Name
    {
      get
      {
        if (string.IsNullOrEmpty(this.FileName))
        {
          return string.Empty;
        }

        return this.FileName.Substring(0, this.FileName.Length - 7);
      }
    }

    public static FastQCBasicStatisticItem ParseFromFile(string filename)
    {
      FastQCBasicStatisticItem result = new FastQCBasicStatisticItem();
      using (StreamReader sr = new StreamReader(filename))
      {
        string line = sr.ReadLine();
        if (line.StartsWith("##FastQC"))
        {
          result.FastQCVersion = line.StringAfter("\t").Trim();
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith(">>Basic Statistics"))
          {
            result.Passed = line.StringAfter("\t").Trim().Equals("pass") ? true : false;
            break;
          }
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith(">>"))
          {
            break;
          }

          var parts = line.Split('\t');
          if (parts[0].Equals("Filename"))
          {
            result.FileName = parts[1];
          }
          else if (parts[0].Equals("File type"))
          {
            result.FileType = parts[1];
          }
          else if (parts[0].Equals("Encoding"))
          {
            result.Encoding = parts[1];
          }
          else if (parts[0].Equals("Total Sequences"))
          {
            result.TotalSequences = long.Parse(parts[1]);
          }
          else if (parts[0].Equals("Filtered Sequences"))
          {
            result.FilteredSequences = long.Parse(parts[1]);
          }
          else if (parts[0].Equals("Sequence length"))
          {
            result.SequenceLength = int.Parse(parts[1]);
          }
          else if (parts[0].Equals("%GC"))
          {
            result.GC = double.Parse(parts[1]);
          }
        }
      }
      return result;
    }
  }
}
