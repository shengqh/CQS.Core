using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA.Gui;
using System;
using System.Collections.Generic;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupParser : ProgressClass, IMpileupParser
  {
    private readonly PileupItemNormalTest _normalTest;
    private readonly PileupProcessorOptions _options;
    private readonly PileupItemParser _parser;
    private readonly PileupItemReadDepthFilter _rdFilter;
    private readonly PileupItemTumorTest _tumorTest;
    private MpileupResult _result;

    public MpileupParser(PileupProcessorOptions options, MpileupResult result)
    {
      _options = options;
      _rdFilter = new PileupItemReadDepthFilter(options.MinimumReadDepth, options.MinimumBaseQuality);
      _normalTest = new PileupItemNormalTest(options.MaximumPercentageOfMinorAlleleInNormal);
      _tumorTest = new PileupItemTumorTest(options.MinimumReadsOfMinorAlleleInTumor, options.MinimumPercentageOfMinorAlleleInTumor);
      _parser = options.GetPileupItemParser();
      _result = result;
    }

    public MpileupFisherResult Parse(string line, bool writeCandidateFile = true)
    {
      var item = _parser.GetValue(line);
      if (item == null)
      {
        _result.MinimumReadDepthFailed++;
        return null;
      }

      if (!_rdFilter.Accept(item))
      {
        _result.MinimumReadDepthFailed++;
        return null;
      }

      //If the bases from all samples are same, ignore the entry.
      if (item.OnlyOneEvent())
      {
        _result.OneEventFailed++;
        return null;
      }

      item.Samples[0].SampleName = "NORMAL";
      item.Samples[1].SampleName = "TUMOR";

      var events = item.GetPairedEvent();

      var fisherresult = item.InitializeTable(events);

      //if (item.SequenceIdentifier.Equals("1") && item.Position == 1663402)
      //{
      //  Console.WriteLine(fisherresult);
      //  Console.WriteLine("{0}\t{1}", fisherresult.Sample1.FailedPercentage, fisherresult.Sample2.FailedPercentage);
      //}

      if (fisherresult.Sample1.FailedPercentage > fisherresult.Sample2.FailedPercentage)
      {
        _result.MinorAlleleDecreasedFailed++;
        return null;
      }

      if (!_tumorTest.Accept(fisherresult))
      {
        _result.MinorAlleleFailedInTumorSample++;
        return null;
      }

      if (!_normalTest.Accept(fisherresult))
      {
        _result.MinorAlleleFailedInNormalSample++;
        return null;
      }

      //group fisher exact test
      fisherresult.CalculateTwoTailPValue();
      if (fisherresult.PValue > _options.FisherPvalue)
      {
        _result.GroupFisherFailed++;
        return null;
      }

      _result.CandidateCount++;

      var result = new MpileupFisherResult
      {
        Item = item,
        Group = fisherresult,
      };

      //save to file
      if (writeCandidateFile)
      {
        //get major and second alleles
        var bases = new HashSet<string>(new[] { events.MajorEvent, events.MinorEvent });
        var piFile = new PileupItemFile(bases);
        result.CandidateFile = string.Format("{0}/{1}.wsm", _options.CandidatesDirectory, MpileupFisherResultFileFormat.GetString(result, '_', false));
        piFile.WriteToFile(result.CandidateFile, item);
      }

      Progress.SetMessage("{0}\t{1}\t{2}", item.SequenceIdentifier, item.Position, fisherresult);

      return result;
    }
  }
}