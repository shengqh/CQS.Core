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
using CQS.Genome.Mapping;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.Genome.Gsnap
{
  public class SAMAlignedItemCandidateGsnapBuilder : AbstractSAMAlignedItemCandidateBuilder
  {
    private int maxMutation = 100;
    private Dictionary<char, char> mutations;

    /// <summary>
    /// Constructor of SAMAlignedItemCandidateBuilder
    /// </summary>
    /// <param name="engineType">1:bowtie1, 2:bowtie2, 3:bwa, 4:gsnap</param>
    public SAMAlignedItemCandidateGsnapBuilder(int maxMutation = 100, Dictionary<char, char> mutations = null)
      : base(4)
    {
      this.maxMutation = maxMutation;
      SetMutations(mutations);
    }

    public SAMAlignedItemCandidateGsnapBuilder(ISAMAlignedItemParserOptions options, Dictionary<char, char> mutations = null)
      : base(options)
    {
      this.maxMutation = options.MaximumNoPenaltyMutationCount;
      SetMutations(mutations);
    }

    private void SetMutations(Dictionary<char, char> mutations)
    {
      if (mutations == null)
      {
        this.mutations = new Dictionary<char, char>();
        this.mutations['C'] = 'T';
        this.mutations['G'] = 'A';
      }
      else
      {
        this.mutations = mutations;
      }
    }

    private static Regex locReg = new Regex(@"([+-])(.+):(\d+)\.\.(\d+)");
    protected override List<T> DoBuild<T>(string fileName, out HashSet<string> totalQueryNames)
    {
      var result = new List<T>();

      totalQueryNames = new HashSet<string>();

      using (var sr = new StreamReader(fileName))
      {
        int count = 0;
        int waitingcount = 0;
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith(">"))
          {
            continue;
          }

          count++;

          if (count % 1000 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          if (count % 100000 == 0)
          {
            Progress.SetMessage("{0} candidates from {1} reads", waitingcount, count);
          }

          //just for test
          //if (waitingcount == 10000)
          //{
          //  break;
          //}

          var parts = line.Split('\t');
          var qname = parts[3];
          totalQueryNames.Add(qname);

          int matchCount = int.Parse(parts[1]);
          if (matchCount == 0)
          {
            continue;
          }

          var seq = parts[0].Substring(1);
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

          var sam = new T()
          {
            Qname = qname,
            Sequence = seq
          };

          for (int i = 0; i < matchCount; i++)
          {
            string matchline = sr.ReadLine();

            if (string.IsNullOrWhiteSpace(matchline))
            {
              sam.ClearLocations();
              break;
            }

            var matchparts = matchline.Split('\t');
            var matchgenome = matchparts[0].Trim();

            if (matchgenome.Contains('-'))//insertion or deletion, not allowed now
            {
              continue;
            }

            var mismatch = matchgenome.Count(m => Char.IsLower(m));
            if (mismatch > _options.MaximumMismatch)
            {
              continue;
            }

            var mutation = matchgenome.Count(m => m == '.');
            if (mutation > this.maxMutation)
            {
              continue;
            }

            var match = locReg.Match(matchparts[2]);
            var strand = match.Groups[1].Value[0];
            var chr = match.Groups[2].Value;
            var start = int.Parse(match.Groups[3].Value);
            var end = int.Parse(match.Groups[4].Value);
            string mismatchPosition = GetMismatchPositions(seq, matchgenome);

            var loc = new SamAlignedLocation(sam)
            {
              Seqname = chr,
              Start = strand == '+' ? start : end,
              End = strand == '+' ? end : start,
              Strand = strand,
              NumberOfMismatch = mismatch,
              NumberOfNoPenaltyMutation = mutation,
              Cigar = matchgenome,
              MismatchPositions = mismatchPosition
            };

            sam.AddLocation(loc);
          }

          if (sam.Locations.Count > 0)
          {
            if (sam.Locations.Count > 1)
            {
              var minNNPM = sam.Locations.Min(m => m.NumberOfNoPenaltyMutation);
              sam.RemoveLocation(m => m.NumberOfNoPenaltyMutation > minNNPM);
            }

            result.Add(sam);
            waitingcount++;
          }
        }

        Progress.SetMessage("Finally, there are {0} candidates from {1} reads", waitingcount, count);
      }

      return result;
    }

    public string GetMismatchPositions(string seq, string matchgenome)
    {
      var misp = new StringBuilder();
      int match = 0;
      for (int i = 0; i < seq.Length; i++)
      {
        if (matchgenome[i] == seq[i])
        {
          match++;
          continue;
        }
        else
        {
          char mismatch = ' ';
          if (matchgenome[i] == '.')
          {
            mismatch = this.mutations[seq[i]];
          }
          else if (char.IsLower(matchgenome[i]))
          {
            mismatch = Char.ToUpper(matchgenome[i]);
          }
          else
          {
            throw new Exception(string.Format("Failed to parse {0} in {1} position of {2}", matchgenome[i], i, matchgenome));
          }
          if (match > 0 || misp.Length == 0)
          {
            misp.Append(match);
          }
          match = 0;
          misp.Append(mismatch);
        }
      }

      if (match > 0)
      {
        misp.Append(match);
      }

      return misp.ToString();
    }
  }
}
