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

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemCandidateBuilder : AbstractSAMAlignedItemCandidateBuilder
  {
    /// <summary>
    /// Constructor of SAMAlignedItemCandidateBuilder
    /// </summary>
    /// <param name="engineType">1:bowtie1, 2:bowtie2, 3:bwa</param>
    public SAMAlignedItemCandidateBuilder(int engineType)
      : base(engineType)
    { }

    public SAMAlignedItemCandidateBuilder(ISAMAlignedItemParserOptions options)
      : base(options)
    { }

    protected override List<T> DoBuild<T>(string fileName, out HashSet<string> totalQueryNames)
    {
      var result = new List<T>();

      _format = _options.GetSAMFormat();

      totalQueryNames = new HashSet<string>();

      using (var sr = SAMFactory.GetReader(fileName, true))
      {
        int count = 0;
        int waitingcount = 0;
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (count % 1000 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          if (count % 100000 == 0 && count > 0)
          {
            Progress.SetMessage("{0} candidates from {1} reads", waitingcount, count);
          }

          count++;

          var parts = line.Split('\t');

          var qname = parts[SAMFormatConst.QNAME_INDEX];
          var seq = parts[SAMFormatConst.SEQ_INDEX];

          totalQueryNames.Add(qname);

          //too short
          if (seq.Length < _options.MinimumReadLength)
          {
            continue;
          }

          //too long
          if (seq.Length > _options.MaximumReadLength)
          {
            continue;
          }

          SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);
          //unmatched
          if (flag.HasFlag(SAMFlags.UnmappedQuery))
          {
            continue;
          }

          var cigar = parts[SAMFormatConst.CIGAR_INDEX];
          ////insertion/deletion
          //if (cigar.Any(m => m == 'I' || m == 'D'))
          //{
          //  continue;
          //}

          //too many mismatchs
          var mismatchCount = _format.GetNumberOfMismatch(parts);
          if (mismatchCount > _options.MaximumMismatch)
          {
            continue;
          }

          bool isReversed = flag.HasFlag(SAMFlags.QueryOnReverseStrand);
          char strand;
          if (isReversed)
          {
            strand = '-';
            seq = SequenceUtils.GetReverseComplementedSequence(seq);
          }
          else
          {
            strand = '+';
          }

          var score = _format.GetAlignmentScore(parts);

          var sam = new T()
          {
            Qname = qname,
            Sequence = seq
          };

          var seqname = parts[SAMFormatConst.RNAME_INDEX].StartsWith("chr") ? parts[SAMFormatConst.RNAME_INDEX].StringAfter("chr") : parts[SAMFormatConst.RNAME_INDEX];
          var loc = new SAMAlignedLocation(sam)
          {
            Seqname = seqname,
            Start = int.Parse(parts[SAMFormatConst.POS_INDEX]),
            Strand = strand,
            Cigar = parts[SAMFormatConst.CIGAR_INDEX],
            NumberOfMismatch = mismatchCount,
            AlignmentScore = score,
            MismatchPositions = _format.GetMismatchPositions(parts)
          };

          loc.ParseEnd(sam.Sequence);
          sam.AddLocation(loc);

          if (_format.HasAlternativeHits)
          {
            _format.ParseAlternativeHits(parts, sam);
          }

          result.Add(sam);

          waitingcount++;
        }

        Progress.SetMessage("Finally, there are {0} candidates from {1} reads", waitingcount, count);
      }

      return result;
    }
  }
}
