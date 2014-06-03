using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupFisherResultFileFormat : IFileFormat<List<MpileupFisherResult>>
  {
    public List<MpileupFisherResult> ReadFromFile(string fileName)
    {
      var result = new List<MpileupFisherResult>();

      foreach (var line in File.ReadAllLines(fileName).Skip(1))
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          var item = new MpileupFisherResult();
          item.ParseString(line, '\t');
          result.Add(item);
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, List<MpileupFisherResult> items)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine(
          "chr\tloc\tref\tmajor_allele\tminor_allele\tnormal_major_count\tnormal_minor_count\ttumor_major_count\ttumor_minor_count\tfisher_group"
          );

        foreach (var res in items)
        {
          sw.WriteLine(res.GetString('\t'));
        }
      }
    }
  }
}
