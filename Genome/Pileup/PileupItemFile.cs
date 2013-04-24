using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.Pileup
{
  /// <summary>
  /// A class used to read/write PileupItem individual base information.
  /// </summary>
  public class PileupItemFile : IFileFormat<PileupItem>
  {
    private static Regex filenameReg = new Regex(@"(.+?)_(\d+)_(\S)");

    public string GetFilename(PileupItem item)
    {
      return string.Format("{0}_{1}_{2}", item.SequenceIdentifier, item.Position, item.Nucleotide);
    }

    public PileupItem ReadFromFile(string fileName)
    {
      var result = new PileupItem();

      var fn = Path.GetFileNameWithoutExtension(fileName);
      var m = filenameReg.Match(fn);
      if (m.Success)
      {
        result.SequenceIdentifier = m.Groups[1].Value;
        result.Position = long.Parse(m.Groups[2].Value);
        result.Nucleotide = m.Groups[3].Value[0];
      }

      using (StreamReader sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        PileupBaseList sample = null;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (sample == null || !parts[0].Equals(sample.SampleName))
          {
            sample = new PileupBaseList();
            sample.SampleName = parts[0];
            result.Samples.Add(sample);
          }

          var curbase = new PileupBase();

          curbase.Event = parts[1];
          curbase.Score = int.Parse(parts[2]);
          curbase.Strand = EnumUtils.StringToEnum<StrandType>(parts[3], StrandType.UNKNOWN);
          curbase.Position = EnumUtils.StringToEnum<PositionType>(parts[4], PositionType.UNKNOWN);
          curbase.EventType = EnumUtils.StringToEnum<EventType>(parts[5], EventType.UNKNOWN);

          sample.Add(curbase);
        }
      }
      return result;
    }

    private HashSet<string> bases;
    public PileupItemFile()
    {
      this.bases = new HashSet<string>();
    }

    public PileupItemFile(HashSet<string> bases)
    {
      this.bases = bases;
    }

    public void WriteToFile(string fileName, PileupItem item)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("SAMPLE\tBase\tScore\tStrand\tPosition\tEvent");

        foreach (var sample in item.Samples)
        {
          foreach (var gg in sample)
          {
            if (bases.Count == 0 || bases.Contains(gg.Event))
            {
              sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", sample.SampleName, gg.Event, gg.Score, gg.Strand, gg.Position, gg.EventType);
            }
          }
        }
      }
    }
  }
}
