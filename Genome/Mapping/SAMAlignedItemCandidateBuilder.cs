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
  public class SAMAlignedItemCandidateBuilder : ProgressClass, ICandidateBuilder
  {
    protected ISAMFormat format;

    protected ISAMAlignedItemParserOptions options;

    /// <summary>
    /// Constructor of SAMAlignedItemCandidateBuilder
    /// </summary>
    /// <param name="engineType">1:bowtie1, 2:bowtie2, 3:bwa</param>
    public SAMAlignedItemCandidateBuilder(int engineType)
    {
      this.options = new SAMAlignedItemParserOptions()
      {
        EngineType = engineType,
        MaximumMismatchCount = int.MaxValue,
        MinimumReadLength = 0,
        Samtools = null
      };
    }

    public SAMAlignedItemCandidateBuilder(ISAMAlignedItemParserOptions options)
    {
      this.options = options;
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
        while ((line= sr.ReadLine()) != null)
        {
          count++;

          if (count % 100 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          var parts = line.Split('\t');

          var qname = parts[SAMFormatConst.QNAME_INDEX];
          var seq = parts[SAMFormatConst.SEQ_INDEX];

          totalQueryNames.Add(qname);

          //too short
          if (seq.Length < options.MinimumReadLength)
          {
            continue;
          }

          //too long
          if (seq.Length > options.MaximumReadLength)
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
          var mismatchCount = format.GetNumberOfMismatch(parts);
          if (mismatchCount > options.MaximumMismatchCount)
          {
            continue;
          }

          bool isReversed = flag.HasFlag(SAMFlags.QueryOnReverseStrand);
          char strand;
          if(isReversed)
          {
            strand  ='-';
            seq = SequenceUtils.GetReverseComplementedSequence(seq);
          }
          else{
            strand = '+';
          }

          var score = format.GetAlignmentScore(parts);

          var sam = new T()
          {
            Qname = qname,
            Sequence = seq
          };

          var loc = new SamAlignedLocation(sam)
          {
            Seqname = parts[SAMFormatConst.RNAME_INDEX].StringAfter("chr"),
            Start = int.Parse(parts[SAMFormatConst.POS_INDEX]),
            Strand = strand,
            Cigar = parts[SAMFormatConst.CIGAR_INDEX],
            NumberOfMismatch = mismatchCount,
            AlignmentScore = score,
            MismatchPositions = format.GetMismatchPositions(parts)
          };

          loc.ParseEnd(sam.Sequence);
          sam.AddLocation(loc);

          if (format.HasAlternativeHits)
          {
            format.ParseAlternativeHits(parts, sam);
          }

          samlist.Add(sam);

          waitingcount++;

          if (waitingcount % 100000 == 0)
          {
            Progress.SetMessage("{0} candidates from {1} reads", waitingcount, count);
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
