using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;

namespace CQS.Genome.SmallRNA
{
  /// <summary>
  /// Remove CCA or CCAA non template additions from reads
  /// </summary>
  public class TrnaNonTemplatedNucleotideAdditionsQueryBuilder : AbstractThreadProcessor
  {
    private TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options;

    public TrnaNonTemplatedNucleotideAdditionsQueryBuilder(TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      this.options = options;
    }

    class CountItem
    {
      public int notNTA { get; set; }
      public int CC { get; set; }
      public int CCA { get; set; }
      public int CCAA { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      if (File.Exists(options.CountFile))
      {
        Progress.SetMessage("Reading count table from " + options.CountFile + " ...");
      }

      var map = options.GetCountMap();
      var dic = new Dictionary<int, CountItem>();

      DoProcess(m => m.SeqString.Length >= options.MinimumReadLength, map, options.OutputFile, dic);

      using (var sw = new StreamWriter(options.SummaryFile))
      {
        sw.WriteLine("Length\tReadsNotNTA\tReadsCC\tReadsCCA\tReadsCCAA");
        var lens = dic.Keys.OrderBy(m => m).ToArray();
        foreach (var len in lens)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", len, dic[len].notNTA, dic[len].CC, dic[len].CCA, dic[len].CCAA);
        }
      }

      Progress.End();

      return result;
    }

    private void DoProcess(Func<FastqSequence, bool> accept, SmallRNACountMap map, string outputFile, Dictionary<int, CountItem> dic)
    {
      Progress.SetMessage("Processing " + options.InputFile + " and writing to " + outputFile + "...");

      var ccaMap = new MapItemReader(0, 1).ReadFromFile(options.CCAFile).ToDictionary(m => m.Key, m => bool.Parse(m.Value.Value));

      var parser = new FastqReader();
      var writer = new FastqWriter();

      StreamWriter swCount = null;
      if (map.HasCountFile)
      {
        swCount = new StreamWriter(outputFile + ".dupcount");
        swCount.WriteLine("Query\tCount\tSequence");
      }

      try
      {
        int readcount = 0;
        var tmpFile = outputFile + ".tmp";
        using (var sr = StreamUtils.GetReader(options.InputFile))
        {
          using (var sw = StreamUtils.GetWriter(tmpFile, outputFile.ToLower().EndsWith(".gz")))
          {
            FastqSequence seq;
            while ((seq = parser.Parse(sr)) != null)
            {
              readcount++;
              if (readcount % 100000 == 0)
              {
                Progress.SetMessage("{0} reads processed", readcount);
              }

              if (!accept(seq))
              {
                continue;
              }

              var name = seq.Name;
              var sequence = seq.SeqString;
              var score = seq.Score;
              var len = sequence.Length;
              var description = seq.Description;
              var count = map.GetCount(seq.Name);

              if (map.HasCountFile)
              {
                swCount.WriteLine("{0}\t{1}\t{2}", seq.Name, count, seq.SeqString);
              }

              CountItem item;
              if (!dic.TryGetValue(sequence.Length, out item))
              {
                item = new CountItem();
                dic[sequence.Length] = item;
              }

              string clipped;
              if (sequence.EndsWith("CCAA"))
              {
                clipped = "CCAA";
                sequence = sequence.Substring(0, sequence.Length - 4);
                item.CCAA += count;
              }
              else if (sequence.EndsWith("CCA"))
              {
                clipped = "CCA";
                sequence = sequence.Substring(0, sequence.Length - 3);
                item.CCA += count;
              }
              else if (sequence.EndsWith("CC"))
              {
                bool isCCA;
                if (ccaMap.TryGetValue(name, out isCCA) && isCCA)
                {
                  clipped = "CC";
                  sequence = sequence.Substring(0, sequence.Length - 2);
                  item.CC += count;
                }
                else
                {
                  clipped = string.Empty;
                  item.notNTA += count;
                }
              }
              else
              {
                clipped = string.Empty;
                item.notNTA += count;
              }

              if (!string.IsNullOrEmpty(clipped))
              {
                var newlen = sequence.Length;
                seq.SeqString = sequence;
                seq.Score = score.Substring(0, newlen);
                seq.Reference = string.Format("{0}{1}{2}", name, SmallRNAConsts.NTA_TAG, clipped);
              }
              else
              {
                seq.Reference = string.Format("{0}{1}", name, SmallRNAConsts.NTA_TAG);
              }
              writer.Write(sw, seq);
              if (map.HasCountFile)
              {
                swCount.WriteLine("{0}\t{1}\t{2}", seq.Name, count, seq.SeqString);
              }
            }
          }
        }

        File.Move(tmpFile, outputFile);
      }
      finally
      {
        if (map.HasCountFile)
        {
          swCount.Close();
        }
      }
    }
  }
}
