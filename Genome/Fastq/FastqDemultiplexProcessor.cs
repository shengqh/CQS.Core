using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Fastq
{
  public class FastqDemultiplexProcessor : AbstractThreadProcessor
  {
    private FastqDemultiplexProcessorOptions options;

    private class BarFile
    {
      public string Barcode { get; set; }
      public string Filename { get; set; }
      public StreamWriter Stream { get; set; }
      public int Count { get; set; }
    }

    public FastqDemultiplexProcessor(FastqDemultiplexProcessorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      Console.WriteLine("Read mapping file " + options.MappingFile + "...");

      var lines = File.ReadAllLines(options.MappingFile).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
      var map = (from l in lines
                 let parts = (from p in l.Split('\t', ' ')
                              let pp = p.Trim()
                              where pp.Length > 0
                              select pp).ToArray()
                 where parts.Length > 1
                 select new BarFile() { Barcode = parts[0], Filename = parts[1] }).ToList();

      var dic = (from k in map
                 select new BarFile() { Barcode = k.Barcode, Filename = Path.Combine(options.OutputDirectory, k.Filename), Stream = null, Count = 0 }).ToDictionary(m => m.Barcode);

      Console.WriteLine("There are " + dic.Count.ToString() + " indecies.");
      foreach (var barcode in dic.Keys.OrderBy(m => m))
      {
        Console.WriteLine("{0}\t{1}", barcode, dic[barcode].Filename);
      }

      try
      {
        result.AddRange(from d in dic select d.Value.Filename);

        var parser = new FastqReader();
        var writer = new FastqWriter();

        var unfound = new Dictionary<string, int>();

        int readcount = 0;
        var reg = new Regex(@".+:\s*(\S+?)\s*$");

        Progress.SetMessage("Processing " + Path.GetFullPath(options.InputFile) + " ...");
        using (var sr = StreamUtils.GetReader(options.InputFile))
        {
          FastqSequence seq;
          while ((seq = parser.Parse(sr)) != null)
          {
            //Console.WriteLine("seq = " + seq.Reference);

            readcount++;
            if (readcount % 100000 == 0)
            {
              Progress.SetMessage("{0} reads processed", readcount);
            }

            var m = reg.Match(seq.Reference);
            if (!m.Success)
            {
              throw new Exception("Cannot find index from " + seq.Reference);
            }

            var barcode = m.Groups[1].Value;
            //Console.WriteLine("barcode = " + barcode);
            BarFile file;
            if (dic.TryGetValue(barcode, out file))
            {
              if (file.Stream == null)
              {
                file.Stream = StreamUtils.GetWriter(file.Filename, file.Filename.ToLower().EndsWith(".gz"));
              }

              if (!options.UntrimTerminalN)
              {
                while (seq.SeqString.Length > 0 && seq.SeqString.Last() == 'N')
                {
                  seq.SeqString = seq.SeqString.Substring(0, seq.SeqString.Length - 1);
                  seq.Score = seq.Score.Substring(0, seq.Score.Length - 1);
                }

                while (seq.SeqString.Length > 0 && seq.SeqString.First() == 'N')
                {
                  seq.SeqString = seq.SeqString.Substring(1);
                  seq.Score = seq.Score.Substring(1);
                }
              }

              writer.Write(file.Stream, seq);
              file.Count++;
            }
            else
            {
              int count;
              if (unfound.TryGetValue(barcode, out count))
              {
                unfound[barcode] = count + 1;
              }
              else
              {
                unfound[barcode] = 1;
                //Console.WriteLine("Barcode " + barcode + " is not defined in map file, ignored.");
              }
            }
          }
        }

        using (var sw = new StreamWriter(Path.Combine(options.OutputDirectory, options.SummaryFile)))
        {
          sw.WriteLine("Type\tIndex\tCount");
          foreach (var d in dic.Keys.OrderBy(m => m))
          {
            sw.WriteLine("Sample\t{0}\t{1}", dic[d].Barcode, dic[d].Count);
          }

          foreach (var d in unfound.OrderByDescending(m => m.Value))
          {
            sw.WriteLine("Unmapped\t{0}\t{1}", d.Key, d.Value);
          }
        }
      }
      finally
      {
        foreach (var d in dic)
        {
          if (null != d.Value.Stream)
          {
            d.Value.Stream.Close();
          }
        }
      }

      Progress.End();

      return result;
    }
  }
}
