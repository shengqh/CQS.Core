using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA.Seq;
using CQS.Genome.Samtools;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationProcessor : AbstractPileupProcessor
  {
    private ValidationProcessorOptions options;
    public ValidationProcessor(ValidationProcessorOptions options)
      : base(options)
    {
      this.options = options;
    }

    protected override bool outputNotCoveredSite { get { return true; } }

    protected override MpileupResult GetMpileupResult()
    {
      var result = new MpileupResult(string.Empty, _options.CandidatesDirectory);

      Progress.SetMessage("Single thread mode ...");
      var parser = _options.GetPileupItemParser(false);
      var pfile = new PileupFile(parser);

      var mutationList = GetValidationList();
      var map = mutationList.Items.ToDictionary(m => GenomeUtils.GetKey(m.Chr, m.Pos));

      switch (_options.From)
      {
        case DataSourceType.Mpileup:
          pfile.Open(_options.MpileupFile);
          break;
        case DataSourceType.BAM:
          var posFile = Path.Combine(_options.CandidatesDirectory, "pos.txt");
          using (var sw = new StreamWriter(posFile))
          {
            mutationList.Items.ForEach(m => sw.WriteLine("{0} {1}", m.Chr, m.Pos));
          }
          var proc = new MpileupProcessor(_options).ExecuteSamtools(new[] { _options.NormalBam, _options.TumorBam }, "", posFile);
          if (proc == null)
          {
            return null;
          }

          pfile.Open(proc.StandardOutput);
          pfile.Samtools = proc;
          break;
        case DataSourceType.Console:
          pfile.Open(Console.In);
          break;
      }

      Progress.SetMessage("Total {0} entries in validation list", mutationList.Items.Length);
      foreach (var m in map)
      {
        Console.WriteLine(m.Key);
      }

      using (pfile)
      {
        try
        {
          IMpileupParser proc = new ValidationParser(_options, result);

          string line;
          while ((line = pfile.ReadLine()) != null)
          {
            result.TotalCount++;

            try
            {
              var locus = parser.GetSequenceIdentifierAndPosition(line);
              var locusKey =  GenomeUtils.GetKey(locus.SequenceIdentifier, locus.Position);

              //Console.WriteLine(locusKey);
              ValidationItem vitem = null;
              if (!map.TryGetValue(locusKey, out vitem))
              {
                throw new Exception(string.Format("Cannot find {0} in validation list : {1}", locusKey, line));
              }

              //Console.WriteLine("Parsing " + line);

              var parres = proc.Parse(line, true);
              if (!string.IsNullOrEmpty(parres.FailedReason))
              {
                Progress.SetMessage("{0}\t{1}\t{2} ~ {3}\t{4}", parres.Item.SequenceIdentifier, parres.Item.Position, parres.Group.Sample1, parres.Group.Sample2, parres.FailedReason);
              }
              result.Results.Add(parres);
            }
            catch (Exception ex)
            {
              var error = string.Format("parsing error {0}\n{1}", ex.Message, line);
              Progress.SetMessage(error);
              Console.Error.WriteLine(ex.StackTrace);
              throw new Exception(error);
            }
          }
        }
        finally
        {
          if (pfile.Samtools != null)
          {
            try
            {
              pfile.Samtools.Kill();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
          }
        }
      }

      result.NotCovered = mutationList.Items.Length - result.TotalCount;
      result.TotalCount = mutationList.Items.Length;

      return result;
    }

    private ValidationFile GetValidationList()
    {
      return new ValidationFile().ReadFromFile(options.ValidationFile);
    }

    protected static string GetLinuxFile(string filename)
    {
      return Path.GetFullPath(filename).Replace("\\", "/");
    }

    public override IEnumerable<string> Process()
    {
      var filterOptions = options.GetFilterOptions();

      if (new FileInfo(_options.BaseFilename).Length > 0)
      {
        if (!filterOptions.PrepareOptions())
        {
          throw new Exception("Filter options failed: " + filterOptions.ParsingErrors.Merge("\n"));
        }

        new FilterProcessor(filterOptions).Process();

        var lines = File.ReadAllLines(filterOptions.ROutputFile).Skip(1).ToArray();
        var glmfailed = lines.Count(m => m.Contains("GLM_PVALUE"));
        using (var sw = new StreamWriter(_options.SummaryFilename, true))
        {
          sw.WriteLine("glm pvalue > {0}\t{1}\t{2}", options.GlmPvalue, glmfailed, lines.Length - glmfailed);
        }
      }

      var mutationList = GetValidationList();
      var candidates = new MpileupFisherResultFileFormat().ReadFromFile(options.CandidatesFilename).ToDictionary(m => GenomeUtils.GetKey(m.Item.SequenceIdentifier, m.Item.Position));
      var items = new FilterItemTextFormat().ReadFromFile(filterOptions.ROutputFile).ToDictionary(m => GenomeUtils.GetKey(m.Chr, m.Start));

      var result = new List<FilterItem>();
      foreach (var mutation in mutationList.Items)
      {
        var key = GenomeUtils.GetKey(mutation.Chr, mutation.Pos);
        if (items.ContainsKey(key))
        {
          result.Add(items[key]);
        }
        else
        {
          var item = new FilterItem();
          item.Chr = mutation.Chr;
          item.Start = mutation.Pos;
          item.End = item.Start;
          item.FisherNormal = string.Empty;
          item.BrglmConverged = string.Empty;
          item.BrglmGroup = 1.0;
          item.BrglmGroupFdr = 1.0;
          item.BrglmScore = string.Empty;
          item.BrglmStrand = string.Empty;
          item.BrglmPosition = string.Empty;
          item.Identity = string.Empty;
          result.Add(item);

          if (candidates.ContainsKey(key))
          {
            var cand = candidates[key];
            item.ReferenceAllele = cand.Item.Nucleotide.ToString();
            item.MajorAllele = cand.Group.SucceedName;
            item.MinorAllele = cand.Group.FailedName;
            item.NormalMajorCount = cand.Group.Sample1.Succeed;
            item.NormalMinorCount = cand.Group.Sample1.Failed;
            item.TumorMajorCount = cand.Group.Sample2.Succeed;
            item.TumorMinorCount = cand.Group.Sample2.Failed;
            item.FisherGroup = cand.Group.PValue;
            item.Filter = cand.FailedReason;
            Console.WriteLine("In candidates : " + item.Filter);
          }
          else
          {
            item.NormalMajorCount = 0;
            item.NormalMinorCount = 0;
            item.TumorMajorCount = 0;
            item.TumorMinorCount = 0;
            item.Filter = "No coverage";
            Console.WriteLine("No read : " + item.Filter);
          }
        }
      }

      new FilterItemVcfWriter(filterOptions).WriteToFile(_options.OutputSuffix + ".vcf", result);
      new FilterItemTextFormat().WriteToFile(_options.OutputSuffix + ".tsv", result);

      return new string[] { _options.OutputSuffix + ".tsv", _options.OutputSuffix + ".vcf" };
    }
  }
}