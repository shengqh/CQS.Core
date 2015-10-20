using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using Bio.IO.SAM;
using System.IO;
using CQS.Genome.Sam;
using RCPA.Seq;
using CQS.Genome.Gtf;

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
      var srItems = SequenceRegionUtils.GetSequenceRegions(options.InputFile).Where(m => options.AcceptName(m.Name)).ToList();

      srItems = (from sr in srItems.GroupBy(m => m.Name)
                 select sr.First()).ToList();

      var keepChrInName = options.KeepChrInName && srItems.Any(m => m.Name.StartsWith("chr"));

      if (!keepChrInName)
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
            if (!keepChrInName)
            {
              name = name.StringAfter("chr");
            }

            List<GtfItem> items;

            if (!srMap.TryGetValue(name, out items))
            {
              if (name.Equals("M"))
              {
                name = "MT";
                srMap.TryGetValue(name, out items);
              }
              else if (name.Equals("chrM"))
              {
                name = "chrMT";
                srMap.TryGetValue(name, out items);
              }
              else if (name.Equals("MT"))
              {
                name = "M";
                srMap.TryGetValue(name, out items);
              }
              else if (name.Equals("chrMT"))
              {
                name = "chrM";
                srMap.TryGetValue(name, out items);
              }
            }

            if (items != null)
            {
              Progress.SetMessage("  there are {0} entries in {1} ...", items.Count, name);
              foreach (var item in items)
              {
                var newseq = seq.SeqString.Substring((int)item.Start - 1, (int)item.Length);
                if (item.Strand == '-')
                {
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
