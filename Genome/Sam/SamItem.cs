using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public class SAMItem : SAMItemSlim
  {
    /// <summary>
    /// bitwise FLAG. Each bit is explained in the following table:
    /// Bit Description
    /// 0x1 template having multiple segments in sequencing
    /// 0x2 each segment properly aligned according to the aligner
    /// 0x4 segment unmapped
    /// 0x8 next segment in the template unmapped
    /// 0x10 SEQ being reverse complemented
    /// 0x20 SEQ of the next segment in the template being reversed
    /// 0x40 the rst segment in the template
    /// 0x80 the last segment in the template
    /// 0x100 secondary alignment
    /// 0x200 not passing quality controls
    /// 0x400 PCR or optical duplicate
    /// Bit 0x4 is the only reliable place to tell whether the segment is unmapped. If 0x4 is set, no 
    /// assumptions can be made about RNAME, POS, CIGAR, MAPQ, bits 0x2, 0x10 and 0x100
    /// and the bit 0x20 of the next segment in the template.
    /// If 0x40 and 0x80 are both set, the segment is part of a linear template, but it is neither the
    /// rst nor the last segment. If both 0x40 and 0x80 are unset, the index of the segment in 
    /// the template is unknown. This may happen for a non-linear template or the index is lost
    /// in data processing.
    /// Bit 0x100 marks the alignment not to be used in certain analyses when the tools in use are
    /// aware of this bit.
    /// If 0x1 is unset, no assumptions can be made about 0x2, 0x8, 0x20, 0x40 and 0x80.
    /// </summary>
    private int _flag;
    public int Flag
    {
      get
      {
        return _flag;
      }
      set
      {
        _flag = value;
        this.Strand = HasFlag(0x10) ? '-' : '+';
      }
    }

    /// <summary>
    /// MAPping Quality. It equals -10log10Pr{mapping position is wrong}, rounded to the
    /// nearest integer. A value 255 indicates that the mapping quality is not available.
    /// </summary>
    public int MapQ { get; set; }

    /// <summary>
    /// IGAR string. The CIGAR operations are given in the following table (set `*' if un-available):
    /// Op BAM Description
    /// M 0 alignment match (can be a sequence match or mismatch)
    /// I 1 insertion to the reference
    /// D 2 deletion from the reference
    /// N 3 skipped region from the reference
    /// S 4 soft clipping (clipped sequences present in SEQ)
    /// H 5 hard clipping (clipped sequences NOT present in SEQ)
    /// P 6 padding (silent deletion from padded reference)
    /// = 7 sequence match
    /// X 8 sequence mismatch
    /// H can only be present as the rst and/or last operation.
    /// S may only have H operations between them and the ends of the CIGAR string.
    /// For mRNA-to-genome alignment, an N operation represents an intron. For other types of
    /// alignments, the interpretation of N is not dened.
    /// </summary>
    public string Cigar { get; set; }

    /// <summary>
    /// Reference sequence name of the NEXT segment in the template. For the last segment,
    /// the next segment is the rst segment in the template. If @SQ header lines are present, RNEXT
    /// (if not `*' or `=') must be present in one of the SQ-SN tag. This eld is set as `*' when the
    /// information is unavailable, and set as `=' if RNEXT is identical RNAME. If not `=' and the next
    /// segment in the template has one primary mapping (see also bit 0x100 in FLAG), this eld is
    /// identical to RNAME of the next segment. If the next segment has multiple primary mappings,
    /// no assumptions can be made about RNEXT and PNEXT. If RNEXT is `*', no assumptions can
    /// be made on PNEXT and bit 0x20.
    /// </summary>
    public string Rnext { get; set; }

    /// <summary>
    /// Position of the NEXT segment in the template. Set as 0 when the information is
    /// unavailable. This eld equals POS of the next segment. If PNEXT is 0, no assumptions can be
    /// made on RNEXT and bit 0x20.
    /// </summary>
    public int Pnext { get; set; }

    /// <summary>
    /// signed observed Template LENgth. If all segments are mapped to the same reference, the
    /// unsigned observed template length equals the number of bases from the leftmost mapped base
    /// to the rightmost mapped base. The leftmost segment has a plus sign and the rightmost has a
    /// minus sign. The sign of segments in the middle is undened. It is set as 0 for single-segment
    /// template or when the information is unavailable.
    /// </summary>
    public int Tlen { get; set; }

    public bool HasFlag(int flag)
    {
      return (this.Flag & flag) == flag;
    }
  }
}
