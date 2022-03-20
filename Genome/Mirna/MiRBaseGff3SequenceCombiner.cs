using CQS.Genome.Gtf;
using RCPA;
using RCPA.Seq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mirna
{
  public class MiRBaseGff3SequenceCombiner : AbstractThreadProcessor
  {
    private MiRBaseGff3SequenceCombinerOptions options;

    public MiRBaseGff3SequenceCombiner(MiRBaseGff3SequenceCombinerOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("reading fasta file ...");
      var faMap = SequenceUtils.Read(new FastaFormat(), options.FastaFile).ToDictionary(m => m.Name);
      Progress.SetMessage("{0} sequences read ...", faMap.Count);

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        Progress.SetMessage("reading gff file ...");
        var gffs = GtfItemFile.ReadFromFile(options.GffFile);
      }

      return new string[] { options.OutputFile };
    }
  }
}
