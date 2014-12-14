using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using System.Threading;
using RCPA.Gui;
using RCPA;
using RCPA.Seq;
using Bio.IO.SAM;
using System.Text.RegularExpressions;

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemCandidateFilterBuilder : ProgressClass, ICandidateBuilder
  {
    protected ISAMFormat format;

    protected ISAMAlignedItemParserOptions options;

    private ISAMAlignedItemParsingFilter _filter;

    /// <summary>
    /// Constructor of SAMAlignedItemCandidateBuilder
    /// </summary>
    /// <param name="engineType">1:bowtie1, 2:bowtie2, 3:bwa</param>
    public SAMAlignedItemCandidateFilterBuilder(int engineType, ISAMAlignedItemParsingFilter filter)
    {
      this.options = new SAMAlignedItemParserOptions()
      {
        EngineType = engineType,
        Samtools = null
      };

      this._filter = filter;
    }

    public SAMAlignedItemCandidateFilterBuilder(ISAMAlignedItemParserOptions options, ISAMAlignedItemParsingFilter filter)
    {
      this.options = options;
      this._filter = filter;
    }

    public List<T> Build<T>(string fileName, out HashSet<string> totalQueryNames) where T : SAMAlignedItem, new()
    {
      format = options.GetSAMFormat();

      var samlist = new List<T>();

      totalQueryNames = new HashSet<string>();

      using (var sr = SAMFactory.GetReader(fileName, options.Samtools, true))
      {
        int count = 0;
        int waitingcount = 0;
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          count++;

          if (count % 1000 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          var parts = line.Split('\t');

          var qname = parts[SAMFormatConst.QNAME_INDEX];
          totalQueryNames.Add(qname);

          if (!_filter.AcceptQueryName(qname))
          {
            continue;
          }

          SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);
          if (!_filter.AcceptFlags(flag))
          {
            continue;
          }

          var seq = parts[SAMFormatConst.SEQ_INDEX];
          if (!_filter.AcceptLength(seq.Length))
          {
            continue;
          }

          var cigar = parts[SAMFormatConst.CIGAR_INDEX];
          if (!_filter.AcceptCigar(cigar))
          {
            continue;
          }

          //too many mismatchs
          var mismatchCount = format.GetNumberOfMismatch(parts);
          if (!_filter.AcceptLength(mismatchCount))
          {
            continue;
          }

          var seqname = parts[SAMFormatConst.RNAME_INDEX].StringAfter("chr");
          var start = int.Parse(parts[SAMFormatConst.POS_INDEX]);
          var end = SAMUtils.ParseEnd(start, cigar);

          bool isReversed = flag.HasFlag(SAMFlags.QueryOnReverseStrand);
          char strand;
          if (isReversed)
          {
            strand = '-';
          }
          else
          {
            strand = '+';
          }

          var sam = new T();
          var loc = new SamAlignedLocation(sam)
          {
            Seqname = seqname,
            Start = start,
            End = end,
            Strand = strand,
          };

          if (!_filter.AcceptLocus(loc))
          {
            continue;
          }

          if (isReversed)
          {
            seq = SequenceUtils.GetReverseComplementedSequence(seq);
          }

          sam.Qname = qname;
          sam.Sequence = seq;

          loc.AlignmentScore = format.GetAlignmentScore(parts);
          loc.Cigar = cigar;
          loc.NumberOfMismatch = mismatchCount;
          loc.MismatchPositions = format.GetMismatchPositions(parts);

          if (format.HasAlternativeHits)
          {
            format.ParseAlternativeHits(parts, sam);
          }

          samlist.Add(sam);

          waitingcount++;

          if (waitingcount % 100 == 0)
          {
            Progress.SetMessage("{0} feature reads from {1} reads", waitingcount, count);
          }
        }
      }

      return DoAddCompleted(samlist);
    }

    protected virtual List<T> DoAddCompleted<T>(List<T> samlist) where T : SAMAlignedItem, new()
    {
      if (options.GetSAMFormat().HasAlternativeHits || samlist.Count == 0)
      {
        return samlist;
      }

      Progress.SetMessage("Sorting mapped reads by name...");
      SAMUtils.SortByName(samlist);
      Progress.SetMessage("Sorting mapped reads by name finished...");

      var result = new List<T>();
      result.Add(samlist[0]);
      T last = samlist[0];

      for (int i = 1; i < samlist.Count; i++)
      {
        var sam = samlist[i];
        if (!last.Qname.Equals(sam.Qname))
        {
          last = sam;
          result.Add(last);
        }
        else
        {
          last.AddLocations(sam.Locations);
          sam.ClearLocations();
        }
      }

      samlist.Clear();
      samlist = null;

      return result;
    }
  }
}
