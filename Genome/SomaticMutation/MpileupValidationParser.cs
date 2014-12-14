using CQS.Genome.Bed;
using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using CQS.Genome.Vcf;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupValidationParser : IMpileupParser
  {
    private readonly PileupOptions _options;
    private readonly PileupItemParser _parser;
    private MpileupResult _result;
    private Dictionary<string, VcfItem> _snps;

    public MpileupValidationParser(PileupOptions options, MpileupResult result)
    {
      _options = options;
      _parser = options.GetPileupItemParser(false);
      _result = result;
      _snps = new VcfItemListFormat().ReadFromFile(_options.ValidationFile).Items.ToDictionary(m => GetKey(m.Seqname, m.Start, m.End));
    }

    private static string GetKey(string chr, long start, long end)
    {
      return string.Format("{0}:{1}-{2}", chr, start, end);
    }

    public MpileupFisherResult Parse(string line)
    {
      var item = _parser.GetSequenceIdentifierAndPosition(line);
      if (item == null)
      {
        return null;
      }

      VcfItem vitem;
      if (!_snps.TryGetValue(GetKey(item.SequenceIdentifier, item.Position, item.Position), out vitem))
      {
        return null;
      }

      item.Samples[0].SampleName = "NORMAL";
      item.Samples[1].SampleName = "TUMOR";

      var events = new PairedEvent(vitem.RefAllele, vitem.AltAllele);

      var fisherresult = item.InitializeTable(events);

      //group fisher exact test
      fisherresult.CalculateTwoTailPValue();

      //get major and second alleles
      var bases = new HashSet<string>(new[] { events.MajorEvent, events.MinorEvent });
      _result.CandidateCount++;

      //save to file
      var piFile = new PileupItemFile(bases);
      var result = new MpileupFisherResult
      {
        Item = item,
        Group = fisherresult,
      };

      result.CandidateFile = string.Format("{0}/{1}.wsm", _options.CandidatesDirectory, result.GetString());
      piFile.WriteToFile(result.CandidateFile, item);

      Console.WriteLine("{0}\t{1}\t{2}", item.SequenceIdentifier, item.Position, fisherresult);

      return result;
    }
  }
}