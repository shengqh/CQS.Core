using System.IO;

namespace CQS.Genome.Fastq
{
  public sealed class FastqWriter
  {
    #region Methods

    public void Write(StreamWriter sw, FastqSequence seq)
    {
      sw.WriteLine("@" + seq.Reference);
      sw.WriteLine(seq.SeqString);
      sw.WriteLine(seq.Strand);
      sw.WriteLine(seq.Score);
    }

    #endregion
  }
}
