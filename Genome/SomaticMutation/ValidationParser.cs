using CQS.Genome.Pileup;
using RCPA.Gui;
using System.Collections.Generic;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationParser : ProgressClass, IMpileupParser
  {
    private readonly PileupItemNormalTest _normalTest;
    private readonly PileupProcessorOptions _options;
    private readonly PileupItemParser _parser;
    private readonly PileupItemReadDepthFilter _rdFilter;
    private readonly PileupItemTumorTest _tumorTest;
    private MpileupResult _result;

    public ValidationParser(PileupProcessorOptions options, MpileupResult result)
    {
      _options = options;
      _parser = options.GetPileupItemParser(false);
      _rdFilter = new PileupItemReadDepthFilter(options.MinimumReadDepth, options.MinimumBaseQuality);
      _normalTest = new PileupItemNormalTest(options.MaximumPercentageOfMinorAlleleInNormal);
      _tumorTest = new PileupItemTumorTest(options.MinimumReadsOfMinorAlleleInTumor, options.MinimumPercentageOfMinorAlleleInTumor);
      _result = result;
    }

    public MpileupFisherResult Parse(string line, bool writeCandidateFile = true)
    {
      MpileupFisherResult result = new MpileupFisherResult();

      var item = _parser.GetValue(line);
      item.Samples[0].SampleName = "NORMAL";
      item.Samples[1].SampleName = "TUMOR";
      var events = item.GetPairedEvent();
      var fisherresult = item.InitializeTable(events);

      result.Item = item;
      result.Group = fisherresult;

      if (!_rdFilter.Accept(item))
      {
        result.FailedReason = _rdFilter.RejectReason;
        _result.MinimumReadDepthFailed++;
        return result;
      }

      //If the bases from all samples are same, ignore the entry.
      if (item.OnlyOneEvent())
      {
        result.FailedReason = "Only one allele detected";
        _result.OneEventFailed++;
        return result;
      }

      if (fisherresult.Sample1.FailedPercentage > fisherresult.Sample2.FailedPercentage)
      {
        result.FailedReason = "MAF decreased in tumor";
        _result.MinorAlleleDecreasedFailed++;
        return result;
      }

      if (!_normalTest.Accept(fisherresult))
      {
        result.FailedReason = _normalTest.RejectReason;
        _result.MinorAlleleFailedInNormalSample++;
        return result;
      }

      if (!_tumorTest.Accept(fisherresult))
      {
        result.FailedReason = _tumorTest.RejectReason;
        _result.MinorAlleleFailedInTumorSample++;
        return result;
      }

      //group fisher exact test
      fisherresult.CalculateTwoTailPValue();
      if (fisherresult.PValue > _options.FisherPvalue)
      {
        result.FailedReason = string.Format("Fisher pvalue > {0}", _options.FisherPvalue);
        _result.GroupFisherFailed++;
        return result;
      }

      if (writeCandidateFile)
      {
        //get major and second alleles
        var bases = new HashSet<string>(new[] { events.MajorEvent, events.MinorEvent });

        //save to file
        var piFile = new PileupItemFile(bases);
        result.CandidateFile = string.Format("{0}/{1}.wsm", _options.CandidatesDirectory, MpileupFisherResultFileFormat.GetString(result, '_', false));
        piFile.WriteToFile(result.CandidateFile, item);
      }

      Progress.SetMessage("{0}\t{1}\t{2}", item.SequenceIdentifier, item.Position, fisherresult);

      return result;
    }
  }
}