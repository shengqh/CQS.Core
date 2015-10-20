using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Genome.Sam;
using Bio.IO.SAM;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Mirna;
using RCPA.Seq;
using CQS.Genome.Gtf;

namespace CQS.Genome.Pileup
{

  public class PileupCountBuilder : AbstractThreadProcessor
  {
    private PileupCountBuilderOptions options;

    public PileupCountBuilder(PileupCountBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      PileupCountList pc = new PileupCountList();

      var format = options.GetSAMFormat();

      var cm = new CountMap(options.CountFile);

      var srItems = SequenceRegionUtils.GetSequenceRegions(options.CoordinateFile, "miRNA", options.BedAsGtf);
      srItems.ForEach(m =>
      {
        m.Seqname = m.Seqname.StringAfter("chr");
      });
      var srmap = srItems.GroupBy(m => m.Seqname).ToDictionary(m => m.Key, m => m.ToList());

      StreamWriter swScript = null;
      try
      {
        if (options.ExportIgvScript)
        {
          swScript = new StreamWriter(options.OutputFile + ".igv");
          swScript.WriteLine("snapshotDirectory {0}", Path.GetDirectoryName(options.OutputFile).Replace('\\', '/'));
        }

        using (StreamWriter sw = new StreamWriter(options.OutputFile))
        {
          sw.WriteLine(@"##fileformat=VCFv4.2
##fileDate={0:yyyyMMdd}
##source={1}
##phasing=partial
##INFO=<ID=NS,Number=1,Type=Integer,Description=""Number of Samples With Data"">
##INFO=<ID=DP,Number=1,Type=Integer,Description=""Total Depth"">
##INFO=<ID=AF,Number=A,Type=Float,Description=""Allele Frequency"">
##INFO=<ID=FP,Number=1,Type=Float,Description=""Fisher Exact Test P-Value"">
##INFO=<ID=MN,Number=.,Type=String,Description=""miRNA name contains this position"">
##FILTER=<ID=FisherET,Description=""Fisher exact test Pvalue less than {2}"">
##FILTER=<ID=AltAlleFreq,Description=""Alternative allele frequency less than {3}"">
##FILTER=<ID=notMiRNA,Description=""Position not located in miRNA locus"">
##FORMAT=<ID=DP,Number=1,Type=Integer,Description=""Read Depth"">
##FORMAT=<ID=AD,Number=1,Type=Integer,Description=""Allelic Depth"">
#CHROM  POS ID  REF ALT QUAL  FILTER  INFO  FORMAT  {4}",
    DateTime.Now,
    "PileupCountBuilder",
    options.FisherPValue,
    options.MinimumAlternativeAlleleFrequency,
    Path.GetFileNameWithoutExtension(options.InputFile));

          using (var sr = SAMFactory.GetReader(options.InputFile, true))
          {
            int count = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
              count++;

              if (count % 100 == 0)
              {
                if (Progress.IsCancellationPending())
                {
                  throw new UserTerminatedException();
                }
              }

              if (count % 100000 == 0)
              {
                Progress.SetMessage("{0} reads processed", count);
              }

              var parts = line.Split('\t');

              var qname = parts[SAMFormatConst.QNAME_INDEX];
              var seq = parts[SAMFormatConst.SEQ_INDEX];

              //too short
              if (seq.Length < options.MinimumReadLength)
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
              //insertion/deletion
              if (cigar.Any(m => m == 'I' || m == 'D'))
              {
                continue;
              }

              var sam = new SAMAlignedItem()
              {
                Qname = qname,
              };

              bool isReversed = flag.HasFlag(SAMFlags.QueryOnReverseStrand);
              char strand;
              if (isReversed)
              {
                strand = '-';
                sam.Sequence = SequenceUtils.GetReverseComplementedSequence(seq);
              }
              else
              {
                strand = '+';
                sam.Sequence = seq;
              }

              var loc = new SamAlignedLocation(sam)
              {
                Seqname = parts[SAMFormatConst.RNAME_INDEX].StringAfter("chr"),
                Start = int.Parse(parts[SAMFormatConst.POS_INDEX]),
                Strand = strand,
                Cigar = parts[SAMFormatConst.CIGAR_INDEX],
                MismatchPositions = format.GetMismatchPositions(parts),
                NumberOfMismatch = format.GetNumberOfMismatch(parts),
                Sequence = seq
              };

              loc.ParseEnd(sam.Sequence);
              sam.AddLocation(loc);

              if (format.HasAlternativeHits)
              {
                format.ParseAlternativeHits(parts, sam);
              }

              var finished = pc.Add(sam, cm.GetCount(sam.Qname));
              if (null == finished || 0 == finished.Count)
              {
                continue;
              }

              foreach (var fin in finished)
              {
                //if (fin.Chromosome.Equals("1") && fin.Position == 5160725)
                //{
                //  Console.WriteLine(fin);
                //}
                var ft = fin.FisherExactTest();
                if (ft.PValue <= options.FisherPValue)
                {
                  var total = fin.Sum(m => m.Value);
                  var minallele = total * options.MinimumAlternativeAlleleFrequency;
                  if (ft.Sample2.Failed >= minallele)
                  {
                    List<GtfItem> srs;
                    List<string> ranges = new List<string>();

                    if (srmap.TryGetValue(sam.Locations[0].Seqname, out srs))
                    {
                      foreach (var seqr in srs)
                      {
                        if (seqr.Contains(fin.Position))
                        {
                          ranges.Add(seqr.GetNameLocation());
                        }
                      }
                    }

                    var alter = (from r in fin
                                 where r.Key != fin.Reference
                                 orderby r.Key
                                 select r).ToList();

                    var str = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\tNS={7};DP={8};AF={9};FP={10:0.##E0}{11}\tDP:AD\t{12}:{13},{14}",
                      fin.Chromosome,
                      fin.Position,
                      ".",
                      fin.Reference,
                      (from r in alter
                       select r.Key.ToString()).Merge(","),
                      0,
                      ranges.Count == 0 ? "notMiRNA" : "PASS",
                      1,
                      total,
                      (from r in alter
                       select string.Format("{0:0.###}", r.Value * 1.0 / total)).Merge(","),
                      ft.PValue,
                      ranges.Count == 0 ? "" : ";" + ranges.Merge(","),
                      total,
                      ft.Sample2.Succeed,
                      (from r in alter
                       select r.Value.ToString()).Merge(","));

                    sw.WriteLine(str);
                    //Console.WriteLine(str);

                    if (swScript != null && ranges.Count > 0)
                    {
                      swScript.WriteLine(@"goto {0}:{1}
sort position
snapshot {0}_{2}_{1}.png", fin.Chromosome, fin.Position, ranges[0].Replace('(', '_').Replace(')', '_').Replace(':', '_'));
                    }
                  }
                }
              }

              finished.Clear();
            }
          }
        }
      }
      finally
      {
        if (swScript != null)
        {
          swScript.Close();
        }
      }
      return new string[] { options.OutputFile };
    }
  }
}
