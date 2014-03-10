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

namespace CQS.Genome.SomaticMutation
{
  public class MpileupFisherResult
  {
    public string CandidateFile { get; set; }
    public PileupItem Item { get; set; }
    public FisherExactTestResult Group { get; set; }
    //public FisherExactTestResult Position { get; set; }
    //public FisherExactTestResult Strand { get; set; }
  }

  public class MpileupParseProcessor
  {
    private readonly PileupItemNormalPercentageTest _normalPercentageTest;
    private readonly PileupOptions _options;
    private readonly PileupItemParser _parser;
    private readonly PileupItemPositionTest _positionTest;
    private readonly PileupItemReadDepthFilter _rdFilter;
    private readonly PileupItemStrandTest _strandTest;
    private readonly PileupItemTumorPercentageTest _tumorPercentageTest;
    public int CandidateCount { get; private set; }
    public int GroupFisherFailed { get; private set; }
    public int MinReadDepthFailed { get; private set; }
    public int OneEventFailed { get; private set; }
    public int MinorAlleleDecreasedFailed { get; private set; }
    public int PercentageFailed { get; private set; }
    public int PositionFisherFailed { get; private set; }
    public int StrandFisherFailed { get; private set; }

    public MpileupParseProcessor(PileupOptions options)
    {
      _options = options;
      _rdFilter = new PileupItemReadDepthFilter(options.MinimumReadDepth, options.MinimumBaseQuality);
      _positionTest = new PileupItemPositionTest();
      _strandTest = new PileupItemStrandTest();
      _normalPercentageTest = new PileupItemNormalPercentageTest(options.MaximumPercentageOfMinorAllele);
      _tumorPercentageTest = new PileupItemTumorPercentageTest(options.MinimumPercentageOfMinorAllele);
      _parser = options.GetPileupItemParser();

      CandidateCount = 0;
      GroupFisherFailed = 0;
      MinReadDepthFailed = 0;
      OneEventFailed = 0;
      MinorAlleleDecreasedFailed = 0;
      PercentageFailed = 0;
      PositionFisherFailed = 0;
      StrandFisherFailed = 0;
    }

    public MpileupFisherResult Parse(string line)
    {
      var item = _parser.GetValue(line);
      if (item == null)
      {
        MinReadDepthFailed++;
        return null;
      }

      if (!_rdFilter.Accept(item))
      {
        MinReadDepthFailed++;
        return null;
      }

      //If the bases from all samples are same, ignore the entry.
      if (item.OnlyOneEvent())
      {
        OneEventFailed++;
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
        MinorAlleleDecreasedFailed++;
        return null;
      }

      if (!_tumorPercentageTest.Accept(fisherresult) || !_normalPercentageTest.Accept(fisherresult))
      {
        PercentageFailed++;
        return null;
      }

      //group fisher exact test
      fisherresult.CalculateTwoTailPValue();
      if (fisherresult.PValue > _options.PValue)
      {
        GroupFisherFailed++;
        return null;
      }

      //get major and second alleles
      var bases = new HashSet<string>(new[] { events.MajorEvent, events.MinorEvent });

      ////position fisher exact test
      //var positionFisher = new FisherExactTestResult();
      //if (!_options.NotFilterPosition)
      //{
      //  positionFisher = _positionTest.Test(item, events);
      //  if (positionFisher.PValue <= _options.PValue)
      //  {
      //    Console.WriteLine(GetInfo("Position failed", item, positionFisher) + "\t" +
      //                      GetInfo("Sample info", item, fisherresult) + "\n" + line);
      //    var positionFile = new PileupItemFile(bases);
      //    var fn = string.Format("{0}/PositionFailed_{1}.wsm", _options.CandidatesDirectory,
      //      positionFile.GetFilename(item));
      //    positionFile.WriteToFile(fn, item);
      //    PositionFisherFailed++;
      //    return null;
      //  }
      //}

      ////strand fisher exact test
      //var strandFisher = new FisherExactTestResult();
      //if (!_options.NotFilterStrand)
      //{
      //  strandFisher = _strandTest.Test(item, events);
      //  if (strandFisher.PValue <= _options.PValue)
      //  {
      //    Console.WriteLine(GetInfo("Strand failed", item, strandFisher) + "\t" +
      //                      GetInfo("Sample info", item, fisherresult) + "\n" + line);
      //    var strandFile = new PileupItemFile(bases);
      //    var fn = string.Format("{0}/StrandFailed_{1}.wsm", _options.CandidatesDirectory, strandFile.GetFilename(item));
      //    strandFile.WriteToFile(fn, item);
      //    StrandFisherFailed++;
      //    return null;
      //  }
      //}

      CandidateCount++;

      //save to file
      var piFile = new PileupItemFile(bases);
      var filename = string.Format("{0}/{1}_{2}_{3}_{4}_{5}.wsm", _options.CandidatesDirectory, item.SequenceIdentifier, item.Position, item.Nucleotide, fisherresult.SucceedName, fisherresult.FailedName);
      piFile.WriteToFile(filename, item);

      Console.Error.WriteLine("{0}\t{1}\t{2}",item.SequenceIdentifier, item.Position,  fisherresult);

      return new MpileupFisherResult
      {
        CandidateFile = filename,
        Item = item,
        Group = fisherresult,
        //Position = positionFisher,
        //Strand = strandFisher
      };
    }

    private static string GetInfo(string name, PileupItem item, FisherExactTestResult positionFisher)
    {
      return string.Format("{0}\t{1}:{2}\t{3}:{4}:{5}\t{6}:{7}:{8}\t{9}:{10}:{11}\t{12}:{13}:{14}",
        name,
        item.SequenceIdentifier, item.Position,
        positionFisher.Sample1.Name, positionFisher.SucceedName, positionFisher.Sample1.Succeed,
        positionFisher.Sample1.Name, positionFisher.FailedName, positionFisher.Sample1.Failed,
        positionFisher.Sample2.Name, positionFisher.SucceedName, positionFisher.Sample2.Succeed,
        positionFisher.Sample2.Name, positionFisher.FailedName, positionFisher.Sample2.Failed);
    }
  }
}