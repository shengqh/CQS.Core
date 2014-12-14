using System.Collections.Generic;
using Bio.IO.SAM;
using CQS.Genome.Sam;
using RCPA.Seq;

namespace CQS.Genome.Pileup
{
  public class AlignedPositionMapBuilder
  {
    private readonly ISAMFile _file;
    private readonly ISAMFormat _format;
    private readonly AlignedPositionMapList _list;
    private readonly AlignedPositionMapBuilderOptions _options;
    private List<AlignedPositionMap> _done;

    public AlignedPositionMapBuilder(AlignedPositionMapBuilderOptions options, string fileName)
    {
      this._options = options;
      _format = options.GetSAMFormat();
      _file = SAMFactory.GetReader(fileName, options.Samtools, true);
      _list = new AlignedPositionMapList();
      _done = new List<AlignedPositionMap>();
    }

    public SAMAlignedItem NextSAMAlignedItem()
    {
      string line;
      while ((line = _file.ReadLine()) != null)
      {
        var parts = line.Split('\t');

        var qname = parts[SAMFormatConst.QNAME_INDEX];
        var seq = parts[SAMFormatConst.SEQ_INDEX];

        var flag = (SAMFlags) int.Parse(parts[SAMFormatConst.FLAG_INDEX]);
        //unmatched
        if (flag.HasFlag(SAMFlags.UnmappedQuery))
        {
          continue;
        }

        //check map quality
        var mapq = int.Parse(parts[SAMFormatConst.MAPQ_INDEX]);
        if (mapq < _options.MinimumReadQuality)
        {
          continue;
        }

        var sam = new SAMAlignedItem
        {
          Qname = qname,
        };

        bool isReversed = flag.HasFlag(SAMFlags.QueryOnReverseStrand);
        char strand;
        if (isReversed)
        {
          strand = '-';
          sam.Sequence = SequenceUtils.GetReverseComplementedSequence(seq);
        }
        else
        {
          strand = '+';
          sam.Sequence = seq;
        }

        var loc = new SamAlignedLocation(sam)
        {
          Seqname = parts[SAMFormatConst.RNAME_INDEX],
          Start = int.Parse(parts[SAMFormatConst.POS_INDEX]),
          Strand = strand,
          Cigar = parts[SAMFormatConst.CIGAR_INDEX],
          MismatchPositions = _format.GetMismatchPositions(parts),
          NumberOfMismatch = _format.GetNumberOfMismatch(parts),
          Sequence = seq,
          Qual = parts[SAMFormatConst.QUAL_INDEX]
        };

        loc.ParseEnd(sam.Sequence);
        sam.AddLocation(loc);

        if (_format.HasAlternativeHits)
        {
          _format.ParseAlternativeHits(parts, sam);
        }

        return sam;
      }

      return null;
    }

    public AlignedPositionMap Next()
    {
      while (_done.Count == 0)
      {
        var sam = NextSAMAlignedItem();
        if (sam == null)
        {
          _done = _list.Positions;
          break;
        }

        _done = _list.Add(sam);
      }

      if (_done.Count > 0)
      {
        var result = _done[0];
        _done.RemoveAt(0);
        return result;
      }

      return null;
    }
  }
}