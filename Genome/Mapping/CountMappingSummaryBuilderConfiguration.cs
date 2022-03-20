using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class CountMappingSummaryBuilderConfiguration
  {
    public List<string> SearchTypes { get; private set; }

    public List<Tuple<string, List<string>>> SampleFiles { get; private set; }

    public static CountMappingSummaryBuilderConfiguration Read(string fileName)
    {
      var result = new CountMappingSummaryBuilderConfiguration();

      var lines = File.ReadAllLines(fileName).Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();

      result.SearchTypes = lines[0].Split('\t').Skip(1).ToList();

      result.SampleFiles = (from l in lines.Skip(1)
                            let parts = l.Split('\t')
                            let sample = parts[0]
                            let files = parts.Skip(1).ToList()
                            select new Tuple<string, List<string>>(sample, files)).ToList();

      return result;
    }
  }
}
