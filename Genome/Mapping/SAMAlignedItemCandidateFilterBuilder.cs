using Bio.IO.SAM;
using CQS.Genome.Sam;
using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemCandidateFilterBuilder : AbstractSAMAlignedItemCandidateBuilder
  {
    private ISAMAlignedItemParsingFilter _filter;

    /// <summary>
    /// Constructor of SAMAlignedItemCandidateBuilder
    /// </summary>
    /// <param name="engineType">1:bowtie1, 2:bowtie2, 3:bwa</param>
    public SAMAlignedItemCandidateFilterBuilder(int engineType, ISAMAlignedItemParsingFilter filter)
      : base(engineType)
    {
      this._filter = filter;
    }

    public SAMAlignedItemCandidateFilterBuilder(ISAMAlignedItemParserOptions options, ISAMAlignedItemParsingFilter filter)
      : base(options)
    {
      this._filter = filter;
    }

    protected override List<T> DoBuild<T>(string fileName, out List<QueryInfo> totalQueries)
    {
      var result = new List<T>();

      _format = _options.GetSAMFormat();

      totalQueries = new List<QueryInfo>();

      using (var sr = SAMFactory.GetReader(fileName, true))
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
          var qi = new QueryInfo(qname);
          totalQueries.Add(qi);

          SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);
          if (!_filter.AcceptFlags(flag))
          {
            continue;
          }

          var mismatchCount = _format.GetNumberOfMismatch(parts);
          var seq = parts[SAMFormatConst.SEQ_INDEX];

          qi.Mismatch = mismatchCount;
          qi.Length = seq.Length;

          //too many mismatchs
          if (!_filter.AcceptMismatch(mismatchCount))
          {
            continue;
          }

          if (!_filter.AcceptQueryName(qname))
          {
            continue;
          }

          if (!_filter.AcceptLength(seq.Length))
          {
            continue;
          }

          var cigar = parts[SAMFormatConst.CIGAR_INDEX];
          if (!_filter.AcceptCigar(cigar))
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
          var loc = new SAMAlignedLocation(sam)
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

          loc.AlignmentScore = _format.GetAlignmentScore(parts);
          loc.Cigar = cigar;
          loc.NumberOfMismatch = mismatchCount;
          loc.MismatchPositions = _format.GetMismatchPositions(parts);

          if (_format.HasAlternativeHits)
          {
            _format.ParseAlternativeHits(parts, sam);
          }

          result.Add(sam);

          waitingcount++;

          if (waitingcount % 100 == 0)
          {
            Progress.SetMessage("{0} feature reads from {1} reads", waitingcount, count);
          }
        }
      }

      return result;
    }
  }
}
