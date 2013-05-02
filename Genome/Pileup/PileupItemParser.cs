using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.Pileup
{
  public class PileupItemParser : IPileupItemParser
  {
    private static HashSet<char> matches;
    private static HashSet<char> ignored;

    static PileupItemParser()
    {
      matches = new HashSet<char>();
      matches.Add('.');
      matches.Add(',');
      matches.Add('A');
      matches.Add('T');
      matches.Add('G');
      matches.Add('C');
      matches.Add('N');
      matches.Add('a');
      matches.Add('t');
      matches.Add('g');
      matches.Add('c');
      matches.Add('n');

      ignored = new HashSet<char>();
      ignored.Add('>');
      ignored.Add('<');
      ignored.Add('*');
    }

    private int minReadDepth;
    private int minBaseMappingQuality;
    private bool ignoreInsertionDeletion;
    private bool ignoreN;
    private bool ignoreTerminal;
    private char minBaseMappingQualityChar;

    private bool DoAcceptReadDepthByScore(string[] parts)
    {
      if (!DoAcceptReadDepth(parts))
      {
        return false;
      }

      for (int scoreIndex = 5; scoreIndex < parts.Length; scoreIndex += 3)
      {
        int count = parts[scoreIndex].Count(m => m >= this.minBaseMappingQualityChar);
        if (count < minReadDepth)
        {
          return false;
        }
      }

      return true;
    }

    private bool DoAcceptReadDepth(string[] parts)
    {
      for (int scoreIndex = 5; scoreIndex < parts.Length; scoreIndex += 3)
      {
        if (parts[scoreIndex].Length < minReadDepth)
        {
          return false;
        }

        var ignoredCount = parts[scoreIndex - 2].Count(m => ignored.Contains(m));
        if (parts[scoreIndex].Length - ignoredCount < minReadDepth)
        {
          return false;
        }
      }

      return true;
    }

    private Func<string[], bool> acceptReadDepth;
    private Func<string[], bool> acceptN;
    private Func<PileupBase, bool> acceptTerminal;
    private Func<PileupBase, bool> acceptScore;

    /// <summary>
    /// Construction of PileupItemParser
    /// </summary>
    /// <param name="minReadDepth">After all other criteria, the minimum read depth for each sample</param>
    /// <param name="minBaseMappingQuality">Minimum base mapping quality</param>
    /// <param name="ignoreInsertionDeletion">Is insertion/deletion ignored?</param>
    /// <param name="ignoreN">Is reference N ignored?</param>
    /// <param name="ignoreTerminal">Is terminal base ignored?</param>
    public PileupItemParser(int minReadDepth, int minBaseMappingQuality, bool ignoreInsertionDeletion, bool ignoreN, bool ignoreTerminal)
    {
      this.minBaseMappingQuality = minBaseMappingQuality;
      if (minBaseMappingQuality > 0)
      {
        this.minBaseMappingQualityChar = (char)(this.minBaseMappingQuality + 33);
        acceptScore = m => m.Score >= this.minBaseMappingQuality;
      }
      else
      {
        this.minBaseMappingQualityChar = (char)0;
        acceptScore = m => true;
      }

      this.minReadDepth = minReadDepth;
      if (minReadDepth > 0)
      {
        if (minBaseMappingQuality > 0)
        {
          acceptReadDepth = DoAcceptReadDepthByScore;
        }
        else
        {
          acceptReadDepth = DoAcceptReadDepth;
        }
      }
      else
      {
        acceptReadDepth = m => true;
      }

      this.ignoreInsertionDeletion = ignoreInsertionDeletion;
      this.ignoreN = ignoreN;

      if (this.ignoreN)
      {
        acceptN = m => m[2][0] != 'N' && m[2][0] != 'n';
      }
      else
      {
        acceptN = m => true;
      }

      this.ignoreTerminal = ignoreTerminal;

      if (ignoreTerminal)
      {
        acceptTerminal = m => m.Position == PositionType.MIDDLE;
      }
      else
      {
        acceptTerminal = m => true;
      }
    }

    protected bool Accept(string[] parts)
    {
      if (parts.Length < 6)
      {
        return false;
      }

      return acceptN(parts) && acceptReadDepth(parts);
    }

    public PileupItem GetSequenceIdentifierAndPosition(string line)
    {
      var parts = line.Split('\t');

      if (!Accept(parts))
      {
        return null;
      }

      PileupItem result = new PileupItem();

      result.SequenceIdentifier = parts[0];
      result.Position = long.Parse(parts[1]);

      return result;
    }

    public PileupItem GetValue(string line)
    {
      var parts = line.Split('\t');

      if (!Accept(parts))
      {
        return null;
      }

      PileupItem result = new PileupItem();

      result.SequenceIdentifier = parts[0];
      result.Position = long.Parse(parts[1]);
      result.Nucleotide = parts[2][0];

      int sampleIndex = 0;
      for (int countIndex = 3; countIndex < parts.Length; countIndex += 3)
      {
        PileupBaseList pbl = new PileupBaseList();

        string seq = parts[countIndex + 1];
        string scores = parts[countIndex + 2];
        int seqLength = seq.Length;

        int baseIndex = 0;
        int scoreIndex = 0;
        while (baseIndex < seqLength)
        {
          //A ’>’ or ’<’ for a reference skip.
          //The deleted bases will be presented as ‘*’ in the following lines. 
          if (seq[baseIndex] == '>' || seq[baseIndex] == '<' || seq[baseIndex] == '*')
          {
            baseIndex++;
            scoreIndex++;
            continue;
          }

          PileupBase pb = new PileupBase();

          //Is it the start of read?
          if (seq[baseIndex] == '^')
          {
            pb.Position = PositionType.START;
            pb.ReadMappingQuality = seq[baseIndex + 1] - 33;
            baseIndex += 2;
            ParseMatchBase(result, pbl, pb, seq, scores, seqLength, ref baseIndex, ref scoreIndex);
          }
          else if (matches.Contains(seq[baseIndex]))
          {
            pb.Position = PositionType.MIDDLE;
            ParseMatchBase(result, pbl, pb, seq, scores, seqLength, ref baseIndex, ref scoreIndex);
          }
          //A pattern ‘\+[0-9]+[ACGTNacgtn]+’ indicates there is an insertion between this reference position and the next reference position. The length of the insertion is given by the integer in the pattern, followed by the inserted sequence. Similarly, a pattern ‘-[0-9]+[ACGTNacgtn]+’ represents a deletion from the reference.
          else if (seq[baseIndex] == '+' || seq[baseIndex] == '-')
          {
            if (ignoreInsertionDeletion)
            {
              //ignore and move to next base
              baseIndex++;

              var num = ParseInsertionDeletionCount(seq, seqLength, ref baseIndex);
              baseIndex += num;
            }
            else
            {
              pb.Position = PositionType.MIDDLE;

              var id = seq[baseIndex];
              pb.EventType = id == '+' ? EventType.INSERTION : EventType.DELETION;
              baseIndex++;

              //get the sequence of insertion/deletion
              var num = ParseInsertionDeletionCount(seq, seqLength, ref baseIndex);
              var idseq = seq.Substring(baseIndex, num);
              pb.Event = string.Format("{0}{1}{2}", id, num, idseq.ToUpper());
              pb.Strand = char.IsUpper(idseq[0]) ? StrandType.FORWARD : StrandType.REVERSE;
              baseIndex += num;

              pbl.Add(pb);
            }

            if (baseIndex < seqLength && seq[baseIndex] == '$')
            {
              pb.Position = PositionType.END;
              baseIndex++;
            }
          }
          else
          {
            throw new Exception(string.Format("I don't know the mean of character {0}", seq[baseIndex]));
          }
        }

        if (pbl.Count < this.minReadDepth)
        {
          return null;
        }

        sampleIndex++;
        pbl.SampleName = "S" + sampleIndex.ToString();
        result.Samples.Add(pbl);
      }

      return result;
    }

    private void ParseMatchBase(PileupItem result, PileupBaseList pbl, PileupBase pb, string seq, string scores, int seqLength, ref int baseIndex, ref int scoreIndex)
    {
      pb.Score = scores[scoreIndex] - 33;
      scoreIndex++;

      //Only the base whose quality passed the criteria will be parsed.
      bool bScorePassed = acceptScore(pb);
      if (bScorePassed)
      {
        //A dot stands for a match to the reference base on the forward strand, 
        if (seq[baseIndex] == '.')
        {
          pb.Strand = StrandType.FORWARD;
          AssignMatch(result, pb);
        }
        //A comma for a match on the reverse strand
        else if (seq[baseIndex] == ',')
        {
          pb.Strand = StrandType.REVERSE;
          AssignMatch(result, pb);
        }
        //‘ACGTN’ for a mismatch on the forward strand and ‘acgtn’ for a mismatch on the reverse strand.
        else
        {
          pb.Strand = char.IsUpper(seq[baseIndex]) ? StrandType.FORWARD : StrandType.REVERSE;
          pb.EventType = EventType.MISMATCH;
          pb.Event = seq[baseIndex].ToString().ToUpper();
        }
      }
      baseIndex++;

      //is it the end of read?
      if (baseIndex < seqLength && seq[baseIndex] == '$')
      {
        pb.Position = PositionType.END;
        baseIndex++;
      }

      if (bScorePassed && acceptTerminal(pb))
      {
        pbl.Add(pb);
      }
    }

    private static void AssignMatch(PileupItem result, PileupBase pb)
    {
      pb.EventType = EventType.MATCH;
      pb.Event = result.UpperNucleotide;
    }

    private static int CheckIsEnd(PileupBase pb, string seq, int seqLength, int baseIndex)
    {
      if (baseIndex < seqLength - 1 && seq[baseIndex + 1] == '$')
      {
        pb.Position = PositionType.END;
        baseIndex++;
      }
      return baseIndex;
    }

    private StrandType GetStrand(char strand)
    {
      if (strand == '.')
      {
        return StrandType.FORWARD;
      }
      else
      {
        return StrandType.REVERSE;
      }
    }

    private static int ParseInsertionDeletionCount(string seq, int seqLength, ref int baseIndex)
    {
      string number = "";
      while (baseIndex < seqLength && char.IsNumber(seq[baseIndex]))
      {
        number = number + seq[baseIndex];
        baseIndex++;
      }
      return int.Parse(number);
    }
  }
}
