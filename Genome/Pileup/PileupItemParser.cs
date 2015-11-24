using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Pileup
{
  public class PileupItemParser : IPileupItemParser
  {
    private static readonly HashSet<char> Matches;
    private static readonly HashSet<char> Ignored;
    private readonly Func<string[], bool> _acceptN;
    private readonly Func<string[], bool> _acceptReadDepth;
    private readonly Func<PileupBase, bool> _acceptScore;
    private readonly Func<PileupBase, bool> _acceptTerminal;

    private readonly bool _ignoreInsertionDeletion;
    private readonly int _minBaseMappingQuality;
    private readonly char _minBaseMappingQualityChar;
    private readonly int _minReadDepth;

    private static int ColumnEachSample = 4;

    static PileupItemParser()
    {
      Matches = new HashSet<char> { '.', ',', 'A', 'T', 'G', 'C', 'N', 'a', 't', 'g', 'c', 'n' };

      Ignored = new HashSet<char> { '>', '<', '*' };
    }

    /// <summary>
    ///   Construction of PileupItemParser
    /// </summary>
    /// <param name="minReadDepth">After all other criteria, the minimum read depth for each sample</param>
    /// <param name="minBaseMappingQuality">Minimum base mapping quality</param>
    /// <param name="ignoreInsertionDeletion">Is insertion/deletion ignored?</param>
    /// <param name="ignoreN">Is reference N ignored?</param>
    /// <param name="ignoreTerminal">Is terminal base ignored?</param>
    public PileupItemParser(int minReadDepth, int minBaseMappingQuality, bool ignoreInsertionDeletion, bool ignoreN,
      bool ignoreTerminal)
    {
      _minBaseMappingQuality = minBaseMappingQuality;
      if (minBaseMappingQuality > 0)
      {
        _minBaseMappingQualityChar = (char)(_minBaseMappingQuality + 33);
        _acceptScore = m => m.Score >= _minBaseMappingQuality;
      }
      else
      {
        _minBaseMappingQualityChar = (char)0;
        _acceptScore = m => true;
      }

      _minReadDepth = minReadDepth;
      if (minReadDepth > 0)
      {
        if (minBaseMappingQuality > 0)
        {
          _acceptReadDepth = DoAcceptReadDepthByScore;
        }
        else
        {
          _acceptReadDepth = DoAcceptReadDepth;
        }
      }
      else
      {
        _acceptReadDepth = m => true;
      }

      _ignoreInsertionDeletion = ignoreInsertionDeletion;

      if (ignoreN)
      {
        _acceptN = m => m[2][0] != 'N' && m[2][0] != 'n';
      }
      else
      {
        _acceptN = m => true;
      }

      if (ignoreTerminal)
      {
        _acceptTerminal = m => m.Position == PositionType.MIDDLE;
      }
      else
      {
        _acceptTerminal = m => true;
      }
    }

    public PileupItem GetSequenceIdentifierAndPosition(string line)
    {
      var parts = line.Split('\t');
      return GetSequenceIdentifierAndPosition(parts);
    }

    public PileupItem GetSequenceIdentifierAndPosition(string[] parts)
    {
      var result = new PileupItem
      {
        SequenceIdentifier = parts[0],
        Position = long.Parse(parts[1])
      };

      return result;
    }

    public bool HasEnoughReads(string[] parts)
    {
      for (var countIndex = 3; countIndex < parts.Length; countIndex += ColumnEachSample)
      {
        if (int.Parse(parts[countIndex]) < _minReadDepth)
        {
          return false;
        }
      }
      return true;
    }

    public bool HasMinorAllele(string[] parts)
    {
      for (var countIndex = 3; countIndex < parts.Length; countIndex += ColumnEachSample)
      {
        var seq = parts[countIndex + 1];
        if (seq.Any(l => char.IsLetter(l)))
        {
          return true;
        }
      }
      return false;
    }

    public PileupItem GetValue(string line)
    {
      var parts = line.Split('\t');
      return GetValue(parts);
    }

    //Get major and minor allele only, without score and position information
    public PileupItem GetSlimValue(string[] parts)
    {
      if (!Accept(parts))
      {
        return null;
      }

      var result = new PileupItem
      {
        SequenceIdentifier = parts[0],
        Position = long.Parse(parts[1]),
        Nucleotide = parts[2][0]
      };

      var sampleIndex = 0;
      for (var countIndex = 3; countIndex < parts.Length; countIndex += ColumnEachSample)
      {
        var pbl = new PileupBaseList();

        var seq = parts[countIndex + 1];
        var seqLength = seq.Length;

        var baseIndex = 0;
        while (baseIndex < seqLength)
        {
          //A ’>’ or ’<’ for a reference skip.
          //The deleted bases will be presented as ‘*’ in the following lines. 
          if (seq[baseIndex] == '>' || seq[baseIndex] == '<' || seq[baseIndex] == '*')
          {
            baseIndex++;
            continue;
          }

          var pb = new PileupBase();

          //Is it the start of read?
          if (seq[baseIndex] == '^')
          {
            baseIndex += 2;
            ParseSlimMatchBase(result, pbl, pb, seq, seqLength, ref baseIndex);
          }
          else if (Matches.Contains(seq[baseIndex]))
          {
            ParseSlimMatchBase(result, pbl, pb, seq, seqLength, ref baseIndex);
          }
          //A pattern ‘\+[0-9]+[ACGTNacgtn]+’ indicates there is an insertion between this reference position and the next reference position. The length of the insertion is given by the integer in the pattern, followed by the inserted sequence. Similarly, a pattern ‘-[0-9]+[ACGTNacgtn]+’ represents a deletion from the reference.
          else if (seq[baseIndex] == '+' || seq[baseIndex] == '-')
          {
            //ignore and move to next base
            baseIndex++;

            var num = ParseInsertionDeletionCount(seq, seqLength, ref baseIndex);
            baseIndex += num;

            if (baseIndex < seqLength && seq[baseIndex] == '$')
            {
              baseIndex++;
            }
          }
          else
          {
            throw new Exception(string.Format("I don't know the mean of character {0}", seq[baseIndex]));
          }
        }

        if (pbl.Count < _minReadDepth)
        {
          return null;
        }

        sampleIndex++;
        pbl.SampleName = "S" + sampleIndex;
        pbl.InitEventCountList();
        result.Samples.Add(pbl);
      }

      return result;
    }

    private void ParseSlimMatchBase(PileupItem result, PileupBaseList pbl, PileupBase pb, string seq, int seqLength, ref int baseIndex)
    {
      //A dot stands for a match to the reference base on the forward strand, 
      switch (seq[baseIndex])
      {
        case '.':
          AssignMatch(result, pb);
          break;
        case ',':
          AssignMatch(result, pb);
          break;
        default:
          pb.EventType = AlignedEventType.MISMATCH;
          pb.Event = seq[baseIndex].ToString().ToUpper();
          break;
      }
      baseIndex++;

      //is it the end of read?
      if (baseIndex < seqLength && seq[baseIndex] == '$')
      {
        baseIndex++;
      }
      pbl.Add(pb);
    }

    public PileupItem GetValue(string[] parts)
    {
      if (!Accept(parts))
      {
        return null;
      }

      var result = new PileupItem
      {
        SequenceIdentifier = parts[0],
        Position = long.Parse(parts[1]),
        Nucleotide = parts[2][0]
      };

      var sampleIndex = 0;
      for (var countIndex = 3; countIndex < parts.Length; countIndex += ColumnEachSample)
      {
        var pbl = new PileupBaseList();

        var seq = parts[countIndex + 1];
        var scores = parts[countIndex + 2];
        var positions = parts[countIndex + 3].Split(',');
        var seqLength = seq.Length;

        var baseIndex = 0;
        var scoreIndex = 0;
        var positionIndex = 0;
        while (baseIndex < seqLength)
        {
          //A ’>’ or ’<’ for a reference skip.
          //The deleted bases will be presented as ‘*’ in the following lines. 
          if (seq[baseIndex] == '>' || seq[baseIndex] == '<' || seq[baseIndex] == '*')
          {
            baseIndex++;
            scoreIndex++;
            positionIndex++;
            continue;
          }

          var pb = new PileupBase();

          //Is it the start of read?
          if (seq[baseIndex] == '^')
          {
            pb.Position = PositionType.START;
            pb.ReadMappingQuality = seq[baseIndex + 1] - 33;
            baseIndex += 2;
            ParseMatchBase(result, pbl, pb, seq, scores, positions, seqLength, ref baseIndex, ref scoreIndex);
          }
          else if (Matches.Contains(seq[baseIndex]))
          {
            pb.Position = PositionType.MIDDLE;
            ParseMatchBase(result, pbl, pb, seq, scores, positions, seqLength, ref baseIndex, ref scoreIndex);
          }
          //A pattern ‘\+[0-9]+[ACGTNacgtn]+’ indicates there is an insertion between this reference position and the next reference position. The length of the insertion is given by the integer in the pattern, followed by the inserted sequence. Similarly, a pattern ‘-[0-9]+[ACGTNacgtn]+’ represents a deletion from the reference.
          else if (seq[baseIndex] == '+' || seq[baseIndex] == '-')
          {
            if (_ignoreInsertionDeletion)
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
              pb.EventType = id == '+' ? AlignedEventType.INSERTION : AlignedEventType.DELETION;
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

        if (pbl.Count < _minReadDepth)
        {
          return null;
        }

        sampleIndex++;
        pbl.SampleName = "S" + sampleIndex;
        pbl.InitEventCountList();
        result.Samples.Add(pbl);
      }

      return result;
    }

    private bool DoAcceptReadDepthByScore(string[] parts)
    {
      if (!DoAcceptReadDepth(parts))
      {
        return false;
      }

      for (var scoreIndex = 5; scoreIndex < parts.Length; scoreIndex += ColumnEachSample)
      {
        var count = parts[scoreIndex].Count(m => m >= _minBaseMappingQualityChar);
        if (count < _minReadDepth)
        {
          return false;
        }
      }

      return true;
    }

    private bool DoAcceptReadDepth(string[] parts)
    {
      for (var scoreIndex = 5; scoreIndex < parts.Length; scoreIndex += ColumnEachSample)
      {
        if (parts[scoreIndex].Length < _minReadDepth)
        {
          return false;
        }

        var ignoredCount = parts[scoreIndex - 2].Count(m => Ignored.Contains(m));
        if (parts[scoreIndex].Length - ignoredCount < _minReadDepth)
        {
          return false;
        }
      }

      return true;
    }

    protected bool Accept(string[] parts)
    {
      if (parts.Length < 6)
      {
        return false;
      }

      return _acceptN(parts) && _acceptReadDepth(parts);
    }

    private void ParseMatchBase(PileupItem result, PileupBaseList pbl, PileupBase pb, string seq, string scores, string[] positions,
      int seqLength, ref int baseIndex, ref int scoreIndex)
    {
      pb.Score = scores[scoreIndex] - 33;
      pb.PositionInRead = positions[scoreIndex];
      scoreIndex++;

      //Only the base whose quality passed the criteria will be parsed.
      bool bScorePassed = _acceptScore(pb);
      if (bScorePassed)
      {
        //A dot stands for a match to the reference base on the forward strand, 
        switch (seq[baseIndex])
        {
          case '.':
            pb.Strand = StrandType.FORWARD;
            AssignMatch(result, pb);
            break;
          case ',':
            pb.Strand = StrandType.REVERSE;
            AssignMatch(result, pb);
            break;
          default:
            pb.Strand = char.IsUpper(seq[baseIndex]) ? StrandType.FORWARD : StrandType.REVERSE;
            pb.EventType = AlignedEventType.MISMATCH;
            pb.Event = seq[baseIndex].ToString().ToUpper();
            break;
        }
      }
      baseIndex++;

      //is it the end of read?
      if (baseIndex < seqLength && seq[baseIndex] == '$')
      {
        pb.Position = PositionType.END;
        baseIndex++;
      }

      if (bScorePassed && _acceptTerminal(pb))
      {
        pbl.Add(pb);
      }
    }

    private static void AssignMatch(PileupItem result, PileupBase pb)
    {
      pb.EventType = AlignedEventType.MATCH;
      pb.Event = result.UpperNucleotide;
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