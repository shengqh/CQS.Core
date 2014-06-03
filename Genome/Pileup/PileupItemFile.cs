using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using RCPA;

namespace CQS.Genome.Pileup
{
  /// <summary>
  ///   A class used to read/write PileupItem individual base information.
  /// </summary>
  public class PileupItemFile : IFileFormat<PileupItem>
  {
    private static readonly Regex FilenameReg = new Regex(@"(.+?)_(\d+)_(\S)");

    private readonly HashSet<string> _bases;

    public PileupItemFile()
    {
      _bases = new HashSet<string>();
    }

    public PileupItemFile(HashSet<string> bases)
    {
      _bases = bases;
    }

    public PileupItem ReadFromFile(string fileName)
    {
      var result = new PileupItem();

      var fn = Path.GetFileNameWithoutExtension(fileName);
      var m = FilenameReg.Match(fn);
      if (m.Success)
      {
        result.SequenceIdentifier = m.Groups[1].Value;
        result.Position = long.Parse(m.Groups[2].Value);
        result.Nucleotide = m.Groups[3].Value[0];
      }

      using (var sr = new StreamReader(fileName))
      {
        sr.ReadLine();
        string line;
        PileupBaseList sample = null;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (sample == null || !parts[0].Equals(sample.SampleName))
          {
            sample = new PileupBaseList { SampleName = parts[0] };
            result.Samples.Add(sample);
          }

          var curbase = new PileupBase
          {
            Event = parts[1],
            Score = int.Parse(parts[2]),
            Strand = EnumUtils.StringToEnum(parts[3], StrandType.UNKNOWN),
            Position = EnumUtils.StringToEnum(parts[4], PositionType.UNKNOWN),
            EventType = EnumUtils.StringToEnum(parts[5], AlignedEventType.UNKNOWN)
          };

          if (parts.Length > 6)
          {
            curbase.PositionInRead = parts[6];
          }

          sample.Add(curbase);
        }
      }
      return result;
    }

    public void WriteToFile(string fileName, PileupItem item)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("SAMPLE\tBase\tScore\tStrand\tPosition\tPositionInRead");

        foreach (var sample in item.Samples)
        {
          foreach (var gg in sample)
          {
            if (_bases.Count == 0 || _bases.Contains(gg.Event))
            {
              sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", sample.SampleName, gg.Event, gg.Score, gg.Strand, gg.Position == PositionType.MIDDLE ? "MIDDLE" : "TERMINAL", gg.PositionInRead);
            }
          }
        }
      }
    }

    public string GetFilename(PileupItem item)
    {
      return string.Format("{0}_{1}_{2}", item.SequenceIdentifier, item.Position, item.Nucleotide);
    }
  }
}