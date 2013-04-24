using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Tophat
{
  public class MutationItemFileReader : IFileReader<List<MutationItem>>
  {
    public List<MutationItem> ReadFromFile(string fileName)
    {
      return (from line in File.ReadAllLines(fileName)
              let parts = line.Split('\t')
              where parts.Length > 5
              select new MutationItem()
              {
                Line = line,
                Name = parts[0],
                Gene = parts[2].StringBefore(":").StringAfter("\"").StringBefore("\""),
                Chr = parts[3],
                Position = int.Parse(parts[4])
              }).ToList();
    }
  }
}
