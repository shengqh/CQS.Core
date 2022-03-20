using CQS.Genome.Fastq;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

      var specificSequencesOnly = options.Sequences != null && options.Sequences.Count > 0;

      var countfiles = options.GetCountFiles();
      var counts = new Dictionary<string, List<SmallRNASequence>>();


      foreach (var file in countfiles)
      {
        var keptNames = new HashSet<string>();
        Func<string[], bool> accept;

        if (specificSequencesOnly)
        {
          foreach (var seq in options.Sequences)
          {
            keptNames.Add(seq);
            Console.WriteLine(seq);
          }
          accept = m => keptNames.Contains(m[2]);
        }
        else if (File.Exists(file.AdditionalFile)) // keep the read in fastq file only
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

          accept = m => keptNames.Contains(m[0]);
        }
        else
        {
          accept = m => true;
        }

        Progress.SetMessage("Reading " + file.File + "...");

        counts[file.Name] = ReadCountFile(file, accept);
      }

      var samples = counts.Keys.OrderBy(m => m).ToArray();

      if (specificSequencesOnly)
      {
        using (var sw = new StreamWriter(options.OutputFile))
        {
          sw.WriteLine("Sequence\t{0}", samples.Merge("\t"));
          foreach (var seq in options.Sequences)
          {
            sw.WriteLine("{0}\t{1}", seq, (from sample in samples
                                           let count = counts[sample]
                                           let find = count.Where(l => l.Sequence.Equals(seq)).FirstOrDefault()
                                           select find == null ? "0" : find.Count.ToString()).Merge("\t"));
          }
        }
      }
      else
      {
        OutputGroup(result, counts, samples);
        var readOutput = Path.ChangeExtension(options.OutputFile, ".read.count");
        var readFormat = new SmallRNASequenceFormat(options.TopNumber, options.ExportFasta);
        readFormat.WriteToFile(readOutput, counts);
        result.Add(readOutput);
      }
      Progress.End();

      return result;
    }

    private void OutputGroup(List<string> result, Dictionary<string, List<SmallRNASequence>> counts, string[] samples)
    {
      var outputFile = options.OutputFile;

      Progress.SetMessage("Building sequence contig...");
      var mergedSequences = SmallRNASequenceUtils.BuildContigByIdenticalSimilarity(counts, options.MinimumOverlapRate, options.MaximumExtensionBase, options.TopNumber);

      Progress.SetMessage("Saving sequence contig...");
      new SmallRNASequenceContigFormat().WriteToFile(options.OutputFile, mergedSequences);
      result.Add(options.OutputFile);

      if (options.ExportContigDetails)
      {
        Progress.SetMessage("Saving sequence contig details...");
        new SmallRNASequenceContigDetailFormat().WriteToFile(options.OutputFile + ".details", mergedSequences);
        result.Add(options.OutputFile + ".details");
      }

      var miniContigFile = Path.ChangeExtension(options.OutputFile, ".minicontig.count");
      Progress.SetMessage("Saving mini sequence contig...");
      var miniContig = SmallRNASequenceUtils.BuildMiniContig(mergedSequences, options.TopNumber);
      new SmallRNASequenceContigFormat().WriteToFile(miniContigFile, miniContig);
      result.Add(miniContigFile);

      if (options.ExportContigDetails)
      {
        Progress.SetMessage("Saving mini sequence contig details...");
        new SmallRNASequenceContigDetailFormat().WriteToFile(miniContigFile + ".details", miniContig);
        result.Add(miniContigFile + ".details");
      }

      if (options.ExportFasta)
      {
        var fastaFile = options.OutputFile + ".fasta";
        Progress.SetMessage("Saving " + fastaFile + " ...");
        new SmallRNASequenceContigFastaFormat(options.TopNumber).WriteToFile(fastaFile, mergedSequences);
        result.Add(fastaFile);

        var miniContigFastaFile = miniContigFile + ".fasta";
        Progress.SetMessage("Saving " + miniContigFastaFile + " ...");
        new SmallRNASequenceContigFastaFormat(options.TopNumber).WriteToFile(miniContigFastaFile, miniContig);
        result.Add(miniContigFastaFile);
      }
    }

    public static List<SmallRNASequence> ReadCountFile(FileItem file, Func<string[], bool> accept)
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
          if (!accept(parts))
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