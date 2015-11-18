using CQS.Genome.Bed;
using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA.Gui;
using RCPA;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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

    private Dictionary<string, List<BedItem>> bedMap = new Dictionary<string, List<BedItem>>();
    protected bool IsIgnored(string seqname, long position)
    {
      List<BedItem> items;
      if (bedMap.TryGetValue(seqname, out items))
      {
        return items.Any(l => l.Contains(position));
      }
      else
      {
        return false;
      }
    }

    public MpileupParser(PileupProcessorOptions options, MpileupResult result)
    {
      _options = options;
      _rdFilter = new PileupItemReadDepthFilter(options.MinimumReadDepth, options.MinimumBaseQuality);
      _normalTest = new PileupItemNormalTest(options.MaximumPercentageOfMinorAlleleInNormal);
      _tumorTest = new PileupItemTumorTest(options.MinimumReadsOfMinorAlleleInTumor, options.MinimumPercentageOfMinorAlleleInTumor);
      _parser = options.GetPileupItemParser();
      _result = result;

      if (File.Exists(_options.ExcludeBedFile))
      {
        bedMap = new BedItemFile<BedItem>().ReadFromFile(_options.ExcludeBedFile).ToGroupDictionary(m => m.Seqname);
      }
    }

    public MpileupFisherResult Parse(string line, bool writeCandidateFile = true)
    {
      PileupItem item;
      if (bedMap.Count > 0)
      {
        var parts = line.Split('\t');
        item = _parser.GetSequenceIdentifierAndPosition(parts);
        if (IsIgnored(item.SequenceIdentifier, item.Position))
        {
          _result.Ignored++;
          return null;
        }
        else
        {
          item = _parser.GetValue(parts);
        }
      }
      else
      {
        item = _parser.GetValue(line);
      }

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