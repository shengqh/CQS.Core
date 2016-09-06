using CQS.Genome.Feature;
using CQS.Genome.Mirna;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableSequenceWriter : ProgressClass
  {
    private int topNumber;
    private double minOverlapRate;
    public SmallRNACountTableSequenceWriter(int topNumber = 100, double minOverlapRate = 0.8)
    {
      this.topNumber = topNumber;
      this.minOverlapRate = minOverlapRate;
    }

    public virtual IEnumerable<string> WriteToFile(string outputFile, List<FeatureItemGroup> features, string removeNamePrefix)
    {
      Dictionary<string, Dictionary<string, SmallRNASequence>> sequences = new Dictionary<string, Dictionary<string, SmallRNASequence>>();
      foreach (var fig in features)
      {
        foreach (var sam in fig)
        {
          foreach (var loc in sam.Locations)
          {
            foreach (var samloc in loc.SamLocations)
            {
              var aligned = samloc.SamLocation.Parent;
              var seq = new SmallRNASequence()
              {
                Sample = aligned.Sample,
                Sequence = aligned.Sequence,
                Count = aligned.QueryCount
              };

              Dictionary<string, SmallRNASequence> ssList;
              if (!sequences.TryGetValue(seq.Sample, out ssList))
              {
                ssList = new Dictionary<string, SmallRNASequence>();
                sequences[seq.Sample] = ssList;
                ssList[seq.Sequence] = seq;
              }
              else
              {
                if (!ssList.ContainsKey(seq.Sequence))
                {
                  ssList[seq.Sequence] = seq;
                }
              }
            }
          }
        }
      }

      var counts = sequences.ToDictionary(m => m.Key, m => m.Value.Values.ToList());
      var samples = sequences.Keys.OrderBy(m => m).ToArray();

      List<SmallRNASequenceContig> mergedSequences = SmallRNASequenceUtils.BuildContigByIdenticalSimilarity(counts, topNumber, minOverlapRate);
      new SmallRNASequenceContigFormat().WriteToFile(outputFile, mergedSequences);
      new SmallRNASequenceContigDetailFormat().WriteToFile(outputFile + ".details", mergedSequences);

      return new[] { outputFile };
    }
  }
}
