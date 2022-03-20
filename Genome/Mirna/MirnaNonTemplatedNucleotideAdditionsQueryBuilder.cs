using CQS.Genome.Fastq;
using CQS.Genome.SmallRNA;
using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MirnaNonTemplatedNucleotideAdditionsQueryBuilder : AbstractThreadProcessor
  {
    private MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options;

    public MirnaNonTemplatedNucleotideAdditionsQueryBuilder(MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var gzipped = options.OutputFile.ToLower().EndsWith(".gz");
      result.Add(options.OutputFile);

      Dictionary<string, FastqSequence> queries = new Dictionary<string, FastqSequence>();
      Progress.SetMessage("Processing " + options.InputFile + " and writing to " + options.OutputFile + "...");
      var parser = new FastqReader();
      var writer = new FastqWriter();

      var map = options.GetCountMap();
      StreamWriter swCount = null;
      if (map.HasCountFile)
      {
        var of = options.OutputFile + ".dupcount";
        swCount = new StreamWriter(of);
        swCount.WriteLine("Query\tCount\tSequence");
      }

      int readcount = 0;
      using (var sr = StreamUtils.GetReader(options.InputFile))
      {
        using (var sw = StreamUtils.GetWriter(options.OutputFile, gzipped))
        {
          FastqSequence seq;
          while ((seq = parser.Parse(sr)) != null)
          {
            readcount++;
            if (readcount % 100000 == 0)
            {
              Progress.SetMessage("{0} reads processed", readcount);
            }
            var name = seq.Name;
            var sequence = seq.SeqString;
            var score = seq.Score;
            var len = sequence.Length;
            var description = seq.Description;
            var count = map.GetCount(seq.Name);

            for (int i = 0; i < 4; i++)
            {
              var newlen = len - i;
              if (newlen < options.MinimumReadLength)
              {
                break;
              }

              string clipped;
              if (i == 0)
              {
                clipped = string.Empty;
              }
              else
              {
                clipped = sequence.Substring(newlen);
              }

              seq.SeqString = sequence.Substring(0, newlen);
              seq.Score = score.Substring(0, newlen);
              seq.Reference = string.Format("{0}{1}{2}", name, SmallRNAConsts.NTA_TAG, clipped);
              writer.Write(sw, seq);
              if (map.HasCountFile)
              {
                swCount.WriteLine("{0}\t{1}\t{2}", seq.Name, count, seq.SeqString);
              }
            }
          }
        }
      }

      if (map.HasCountFile)
      {
        swCount.Close();
      }

      Progress.End();

      return result;
    }
  }
}
