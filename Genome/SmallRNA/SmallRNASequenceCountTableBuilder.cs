using CQS.Genome.Fastq;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilder : AbstractThreadProcessor
  {
    public Func<FastqSequence, bool> Accept { get; set; }

    private SmallRNASequenceCountTableBuilderOptions options;

    public SmallRNASequenceCountTableBuilder(SmallRNASequenceCountTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var countfiles = options.GetCountFiles();
      var counts = new Dictionary<string, List<SmallRNASequence>>();
      foreach (var file in countfiles)
      {
        var keptNames = new HashSet<string>();
        Func<string, bool> accept;
        if (File.Exists(file.AdditionalFile)) // keep the read in fastq file only
        {
          Progress.SetMessage("Reading " + file.AdditionalFile + "...");
          var fastqReader = new FastqReader();
          using (var sr = StreamUtils.GetReader(file.AdditionalFile))
          {
            FastqSequence fs;
            while ((fs = fastqReader.Parse(sr)) != null)
            {
              var curname = fs.Name.StringBefore(SmallRNAConsts.NTA_TAG);
              //Console.Error.WriteLine(curname);
              keptNames.Add(curname);
            }
          }

          accept = m => keptNames.Contains(m);
        }
        else
        {
          accept = m => true;
        }

        Progress.SetMessage("Reading " + file.File + "...");

        counts[file.Name] = ReadCountFile(file, accept);
      }

      var samples = counts.Keys.OrderBy(m => m).ToArray();

      OutputGroup(result, counts, samples);

      var readOutput = Path.ChangeExtension(options.OutputFile, ".read.count");
      var readFormat = new SmallRNASequenceFormat(options.TopNumber, options.ExportFasta);
      readFormat.WriteToFile(readOutput, counts);
      result.Add(readOutput);

      Progress.End();

      return result;
    }

    private void OutputGroup(List<string> result, Dictionary<string, List<SmallRNASequence>> counts, string[] samples)
    {
      var outputFile = options.OutputFile;
      var topNumber = options.TopNumber;
      var minOverlapRate = options.MinimumOverlapRate;

      Progress.SetMessage("Building sequence contig...");
      var mergedSequences = SmallRNASequenceUtils.BuildContigByIdenticalSimilarity(counts, topNumber, minOverlapRate);

      Progress.SetMessage("Saving sequence contig...");
      new SmallRNASequenceContigFormat().WriteToFile(options.OutputFile, mergedSequences);

      Progress.SetMessage("Saving sequence contig details...");
      new SmallRNASequenceContigDetailFormat().WriteToFile(options.OutputFile + ".details", mergedSequences);

      result.Add(options.OutputFile);
      result.Add(options.OutputFile + ".details");

      if (options.ExportFasta)
      {
        var fastaFile = options.OutputFile + ".fasta";
        Progress.SetMessage("Saving " + fastaFile + " ...");
        new SmallRNASequenceContigFastaFormat(options.TopNumber).WriteToFile(fastaFile, mergedSequences);
        result.Add(fastaFile);
      }
    }

    public static List<SmallRNASequence> ReadCountFile(FileItem file, Func<string, bool> accept)
    {
      var countList = new List<SmallRNASequence>();
      using (var sr = new StreamReader(file.File))
      {
        var header = sr.ReadLine();
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (parts.Length < 3)
          {
            continue;
          }
          var query = parts[0];
          if (!accept(query))
          {
            continue;
          }

          var seq = new SmallRNASequence()
          {
            Sample = file.Name,
            Sequence = parts[2],
            Count = int.Parse(parts[1])
          };

          countList.Add(seq);
        }
      }

      return countList;
    }
  }
}