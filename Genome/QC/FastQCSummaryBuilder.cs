using RCPA;
using RCPA.R;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.QC
{
  public class FastQCSummaryBuilder : AbstractThreadProcessor
  {
    private FastQCSummaryBuilderOptions options;

    public FastQCSummaryBuilder(FastQCSummaryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      result.AddRange(SummarizeBasicResult());
      result.AddRange(SummarizeCount());

      return result;
    }

    private List<string> SummarizeCount()
    {
      var result = new List<string>();
      var datafile = Path.ChangeExtension(options.OutputFile, ".reads.tsv");

      var qcitems = (from dir in Directory.GetDirectories(options.InputDir)
                     select new
                     {
                       Sample = Path.GetFileName(dir),
                       Data = (from subdir in Directory.GetDirectories(dir, "*_fastqc")
                               let dataFile = Path.Combine(subdir, "fastqc_data.txt")
                               let line = File.ReadAllLines(dataFile).Where(m => m.StartsWith("Total Sequences")).First()
                               select int.Parse(line.StringAfter("\t"))).ToArray()
                     }).ToList();

      var errors = qcitems.Where(m => m.Data.Any(l => l != m.Data.First())).ToArray();
      if(errors.Length > 0)
      {
        var errorFile = datafile + ".error";
        using (var sw = new StreamWriter(errorFile))
        {
          sw.WriteLine("Sample\tRead1\tRead2");
          foreach (var qc in errors)
          {
            sw.WriteLine("{0}\t{1}\t{2}", qc.Sample, qc.Data[0], qc.Data[1]);
          }
        }
        result.Add(errorFile);
      }

      using (var sw = new StreamWriter(datafile))
      {
        sw.WriteLine("Sample\tReads");
        foreach (var qc in qcitems)
        {
          sw.WriteLine("{0}\t{1}", qc.Sample, qc.Data.First());
        }
      }

      result.Add(options.OutputFile);

      var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/fastqc_summary_count.r").FullName;
      if (File.Exists(rfile))
      {
        var targetrfile = datafile + ".r";
        var resultfile = datafile + ".png";
        using (var sw = new StreamWriter(targetrfile))
        {
          sw.NewLine = "\n";
          sw.WriteLine("outputdir<-\"{0}\"", Path.GetDirectoryName(Path.GetFullPath(options.OutputFile)).Replace("\\", "/"));
          sw.WriteLine("inputfile<-\"{0}\"", Path.GetFileName(datafile));
          sw.WriteLine("outputfile<-\"{0}\"", Path.GetFileName(resultfile));
          string line = File.ReadAllText(rfile);
          using (var sr = new StreamReader(rfile))
          {
            if (line.Contains("#predefine_end"))
            {
              while ((line = sr.ReadLine()) != null)
              {
                if (line.Contains("#predefine_end"))
                {
                  break;
                }
              }
            }

            while ((line = sr.ReadLine()) != null)
            {
              sw.WriteLine(line);
            }
          }
        }
        SystemUtils.Execute("R", "--vanilla --slave -f \"" + targetrfile + "\"");

        if (File.Exists(resultfile))
        {
          result.Add(resultfile);
        }
      }

      return result;
    }

    private List<string> SummarizeBasicResult()
    {
      var result = new List<string>();
      var qcitems = (from dir in Directory.GetDirectories(options.InputDir)
                     select new
                     {
                       Sample = Path.GetFileName(dir),
                       Data = (from subdir in Directory.GetDirectories(dir, "*_fastqc")
                               let summaryFile = Path.Combine(subdir, "summary.txt")
                               select FastQCSummaryItem.ReadFromFile(summaryFile)).ToArray()
                     }).ToList();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("File\tCategory\tQCResult");
        foreach (var qc in qcitems)
        {
          var filename = qc.Sample;
          WriteData(sw, filename, "Basic Statistics", qc.Data, m => m.BasicStatistics);
          WriteData(sw, filename, "Per base sequence quality", qc.Data, m => m.PerBaseSequenceQuality);
          WriteData(sw, filename, "Per tile sequence quality", qc.Data, m => m.PerTileSequenceQuality);
          WriteData(sw, filename, "Per sequence quality scores", qc.Data, m => m.PerSequenceQualityScore);
          WriteData(sw, filename, "Per base sequence content", qc.Data, m => m.PerBaseSequenceContent);
          WriteData(sw, filename, "Per sequence GC content", qc.Data, m => m.PerSequenceGCContent);
          WriteData(sw, filename, "Per base N content", qc.Data, m => m.PerBaseNContent);
          WriteData(sw, filename, "Sequence Length Distribution", qc.Data, m => m.SequenceLengthDistribution);
          WriteData(sw, filename, "Sequence Duplication Levels", qc.Data, m => m.SequenceDuplicatonLevels);
          WriteData(sw, filename, "Overrepresented sequences", qc.Data, m => m.OverrepresentedSequences);
          WriteData(sw, filename, "Adapter Content", qc.Data, m => m.AdapterContent);
        }
      }

      result.Add(options.OutputFile);

      var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/fastqc_summary.r").FullName;
      if (File.Exists(rfile))
      {
        var targetrfile = options.OutputFile + ".r";
        var resultfile = options.OutputFile + ".png";
        using (var sw = new StreamWriter(targetrfile))
        {
          sw.NewLine = "\n";
          sw.WriteLine("outputdir<-\"{0}\"", Path.GetDirectoryName(Path.GetFullPath(options.OutputFile)).Replace("\\", "/"));
          sw.WriteLine("inputfile<-\"{0}\"", Path.GetFileName(options.OutputFile));
          sw.WriteLine("outputfile<-\"{0}\"", Path.GetFileName(resultfile));
          string line = File.ReadAllText(rfile);
          using (var sr = new StreamReader(rfile))
          {
            if (line.Contains("#predefine_end"))
            {
              while ((line = sr.ReadLine()) != null)
              {
                if (line.Contains("#predefine_end"))
                {
                  break;
                }
              }
            }

            while ((line = sr.ReadLine()) != null)
            {
              sw.WriteLine(line);
            }
          }
        }
        SystemUtils.Execute("R", "--vanilla --slave -f \"" + targetrfile + "\"");

        if (File.Exists(resultfile))
        {
          result.Add(resultfile);
        }
      }

      return result;
    }

    private void WriteData(StreamWriter sw, string sample, string category, FastQCSummaryItem[] items, Func<FastQCSummaryItem, FastQCType> getValue)
    {
      if (items.Any(m => getValue(m).Equals(FastQCType.FAIL)))
      {
        sw.WriteLine("{0}\t{1}\t{2}", sample, category, FastQCType.FAIL);
      }
      else if (items.Any(m => getValue(m).Equals(FastQCType.WARN)))
      {
        sw.WriteLine("{0}\t{1}\t{2}", sample, category, FastQCType.WARN);
      }
      else if (items.Any(m => getValue(m).Equals(FastQCType.PASS)))
      {
        sw.WriteLine("{0}\t{1}\t{2}", sample, category, FastQCType.PASS);
      }
    }
  }
}
