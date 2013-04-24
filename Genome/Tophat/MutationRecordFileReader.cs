using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Tophat
{
  public class MutationRecordFileReader : IFileReader<List<MutationItem>>
  {
    public List<MutationItem> ReadFromFile(string fileName)
    {
      List<MutationItem> result = new List<MutationItem>();
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i += 3)
      {
        var line = lines[i];
        if (line.Trim().Length == 0)
        {
          break;
        }
        var parts = line.Split('\t');

        result.Add(new MutationItem()
              {
                Line = line,
                Name = parts[3],
                Chr = parts[4],
                Position = int.Parse(parts[5])
              });
      }
      return result;
    }
  }
}
