using System;
using System.IO;
using System.Linq;

namespace CQS.Genome.QC
{
  public enum FastQCType { UNKNOWN, PASS, WARN, FAIL };

  public class FastQCSummaryItem
  {
    public string FileName { get; set; }
    public FastQCType BasicStatistics { get; set; }
    public FastQCType PerBaseSequenceQuality { get; set; }
    public FastQCType PerTileSequenceQuality { get; set; }
    public FastQCType PerSequenceQualityScore { get; set; }
    public FastQCType PerBaseSequenceContent { get; set; }
    public FastQCType PerSequenceGCContent { get; set; }
    public FastQCType PerBaseNContent { get; set; }
    public FastQCType SequenceLengthDistribution { get; set; }
    public FastQCType SequenceDuplicatonLevels { get; set; }
    public FastQCType OverrepresentedSequences { get; set; }
    public FastQCType AdapterContent { get; set; }
    public FastQCType KmerContent { get; set; }

    public void Read(string fileName)
    {
      var lines = File.ReadAllLines(fileName);
      this.FileName = lines[0].StringAfter("Basic Statistics").Trim();
      this.BasicStatistics = GetQCType(lines, "Basic Statistics");
      this.PerBaseSequenceQuality = GetQCType(lines, "Per base sequence quality");
      this.PerTileSequenceQuality = GetQCType(lines, "Per tile sequence quality");
      this.PerSequenceQualityScore = GetQCType(lines, "Per sequence quality scores");
      this.PerBaseSequenceContent = GetQCType(lines, "Per base sequence content");
      this.PerSequenceGCContent = GetQCType(lines, "Per sequence GC content");
      this.PerBaseNContent = GetQCType(lines, "Per base N content");
      this.SequenceLengthDistribution = GetQCType(lines, "Sequence Length Distribution");
      this.SequenceDuplicatonLevels = GetQCType(lines, "Sequence Duplication Levels");
      this.OverrepresentedSequences = GetQCType(lines, "Overrepresented sequences");
      this.AdapterContent = GetQCType(lines, "Adapter Content");
      this.KmerContent = GetQCType(lines, "Kmer Content");
    }

    private FastQCType GetQCType(string[] lines, string p)
    {
      var line = lines.Where(l => l.Contains(p)).FirstOrDefault();
      if (line == null)
      {
        Console.Error.WriteLine("Cannot find key {p}!");
        return FastQCType.UNKNOWN;
      }

      var qctype = line.Split('\t').First();
      return (FastQCType)(Enum.Parse(FastQCType.PASS.GetType(), qctype));
    }

    public static FastQCSummaryItem ReadFromFile(string fileName)
    {
      var result = new FastQCSummaryItem();
      result.Read(fileName);
      return result;
    }
  }
}
