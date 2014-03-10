using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using RCPA.Seq;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MirnaToDNAConverter : AbstractThreadFileProcessor
  {
    public MirnaToDNAConverter()
    { }

    public override IEnumerable<string> Process(string fileName)
    {
      Progress.SetMessage("Loading sequences from " + fileName + "...");
      var seqs = SequenceUtils.Read(new FastaFormat(), fileName);
      Progress.SetMessage("Converint {0} sequences ...", seqs.Count);
      seqs.ForEach(m =>
      {
        m.SeqString = MiRnaToDna(m.SeqString);
      });

      var result = Path.ChangeExtension(fileName, ".dna" + Path.GetExtension(fileName));

      Progress.SetMessage("Saving {0} sequences to {1}", seqs.Count, result);
      SequenceUtils.Write(new FastaFormat(), result, seqs);

      Progress.SetMessage("Finished!");
      return new string[] { result };
    }

    private string MiRnaToDna(string p)
    {
      return p.Replace('U', 'T').Replace('u', 't');
    }
  }
}
