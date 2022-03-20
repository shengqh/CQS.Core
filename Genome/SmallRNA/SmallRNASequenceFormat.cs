using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceFormat : IFileFormat<Dictionary<string, List<SmallRNASequence>>>
  {
    private int topNumber;
    private bool exportFasta;

    public SmallRNASequenceFormat(int topNumber, bool exportFasta)
    {
      this.topNumber = topNumber;
      this.exportFasta = exportFasta;
    }

    public Dictionary<string, List<SmallRNASequence>> ReadFromFile(string fileName)
    {
      var result = new Dictionary<string, List<SmallRNASequence>>();
      using (var sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var samples = line.Split('\t').Skip(1).ToArray();
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          var sequence = parts[0];
          for (int i = 1; i < parts.Length; i++)
          {
            var count = int.Parse(parts[i]);
            if (count == 0)
            {
              continue;
            }
            var sample = samples[i - 1];
            List<SmallRNASequence> seqList;
            if (!result.TryGetValue(sample, out seqList))
            {
              seqList = new List<SmallRNASequence>();
              result[sample] = seqList;
            }
            seqList.Add(new SmallRNASequence()
            {
              Sample = sample,
              Sequence = sequence,
              Count = count
            });
          }
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, Dictionary<string, List<SmallRNASequence>> counts)
    {
      var mergedSequences = SmallRNASequenceUtils.BuildContigByIdenticalSequence(counts, this.topNumber);
      new SmallRNASequenceContigFormat().WriteToFile(fileName, mergedSequences);

      if (this.exportFasta)
      {
        var fastaFile = fileName + ".fasta";
        new SmallRNASequenceContigFastaFormat(this.topNumber).WriteToFile(fastaFile, mergedSequences);
      }
    }
  }
}
