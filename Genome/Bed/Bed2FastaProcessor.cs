using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using Bio.IO.SAM;
using System.IO;
using CQS.Genome.Sam;
using RCPA.Seq;

namespace CQS.Genome.Bed
{
  public class Bed2FastaProcessor : AbstractThreadProcessor
  {
    private Bed2FastaProcessorOptions options;
    public Bed2FastaProcessor(Bed2FastaProcessorOptions option)
    {
      this.options = option;
    }

    public override IEnumerable<string> Process()
    {
      var srItems = SequenceRegionUtils.GetSequenceRegions(options.InputFile);

      if (!options.KeepChrInName)
      {
        srItems.ForEach(m => m.Seqname = m.Seqname.StringAfter("chr"));
      }

      var srMap = srItems.ToGroupDictionary(m => m.Seqname);

      var ff = new FastaFormat();
      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        using (StreamReader sr = new StreamReader(options.GenomeFastaFile))
        {
          Sequence seq;
          while ((seq = ff.ReadSequence(sr)) != null)
          {
            Progress.SetMessage("processing " + seq.Name + " ...");
            var name = seq.Name;
            if (!options.KeepChrInName)
            {
              name = name.StringAfter("chr");
            }

            if (srMap.ContainsKey(name))
            {
              var items = srMap[name];
              Progress.SetMessage("  there are {0} entries in {1} ...", items.Count, name);
              foreach (var item in items)
              {
                var newseq = seq.SeqString.Substring((int)item.Start - 1, (int)item.Length);
                if(item.Strand == '-'){
                  newseq = SequenceUtils.GetReverseComplementedSequence(newseq);
                }
                newseq = newseq.ToUpper();

                var newname = string.Format("{0} {1} {2}", item.Name, item.GetLocationWithoutStrand(), item.Strand);
                var entry = new Sequence(newname, newseq);

                ff.WriteSequence(sw, entry);
              }
            }
          }
        }
      }
      return new string[] { options.OutputFile };
    }
  }
}
