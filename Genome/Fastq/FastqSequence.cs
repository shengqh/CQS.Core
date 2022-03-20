using RCPA.Seq;
using System.Collections.Generic;

namespace CQS.Genome.Fastq
{
  public class FastqSequence : Sequence
  {
    public FastqSequence(string reference, string seqString)
      : base(reference, seqString)
    {
      this.RepeatCount = 1;
      this.RepeatScores = new List<string>();
    }

    public char Strand { get; set; }

    public string Score { get; set; }

    public int RepeatCount { get; set; }

    public List<string> RepeatScores { get; private set; }
  }
}
