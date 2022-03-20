using CQS.Genome.Bed;
using CQS.Genome.Pileup;
using RCPA;
using RCPA.Gui;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupParser : ProgressClass, IMpileupParser
  {
    private readonly PileupItemNormalTest _normalTest;
    private readonly PileupProcessorOptions _options;
    private readonly PileupItemParser _parser;
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
      var parts = line.Split('\t');
      if (bedMap.Count > 0)
      {
        var sp = _parser.GetSequenceIdentifierAndPosition(parts);
        if (IsIgnored(sp.SequenceIdentifier, sp.Position))
        {
          _result.Ignored++;
          return null;
        }
      }

      //for debug
      //var sptemp = _parser.GetSequenceIdentifierAndPosition(parts);
      //if (sptemp.SequenceIdentifier == "2" && sptemp.Position == 89161431)
      //{
      //  var debugFile = string.Format("{0}/debug.txt", _options.CandidatesDirectory);
      //  File.WriteAllLines(debugFile, new[] { line });
      //  Console.WriteLine("Catched the line");
      //  System.Windows.Forms.Application.Exit();
      //}

      //didn't consider minimum score requirement
      if (!_parser.HasEnoughReads(parts))
      {
        _result.MinimumReadDepthFailed++;
        return null;
      }

      //didn't consider minimum score requirement
      if (!_parser.HasMinorAllele(parts))
      {
        _result.OneEventFailed++;
        return null;
      }

      //parsing full result considering score limitation
      var item = _parser.GetValue(parts);
      if (item == null)
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
      if (_options.UseZeroMinorAlleleStrategy && fisherresult.Sample1.Failed == 0)
      {
        //Console.WriteLine("UseZeroMinorAlleleStrategy : {0}", fisherresult);
        if (fisherresult.PValue > _options.ZeroMinorAlleleStrategyFisherPvalue)
        {
          _result.GroupFisherFailed++;
          return null;
        }
      }
      else if (fisherresult.PValue > _options.FisherPvalue)
      {
        _result.GroupFisherFailed++;
        return null;
      }

      //passed all criteria
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