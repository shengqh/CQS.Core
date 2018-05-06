using CQS.Genome.Bed;
using CQS.Genome.Gtf;
using LumenWorks.Framework.IO.Csv;
using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNADatabaseBuilder : AbstractThreadProcessor
  {

    private SmallRNADatabaseBuilderOptions options;

    public SmallRNADatabaseBuilder(SmallRNADatabaseBuilderOptions options)
    {
      this.options = options;
    }

    private string GetTRNAName(string oldName)
    {
      var result = oldName;
      while (result.Contains("_"))
      {
        result = result.StringAfter("_");
      }

      return "tRNA:" + result;
    }

    public override IEnumerable<string> Process()
    {
      var paramFile = options.OutputFile + ".param";
      options.SaveToFile(options.OutputFile + ".param");

      var bedfile = new BedItemFile<BedItem>(6);

      Dictionary<string, string> chrNameMap = new Dictionary<string, string>();
      var ff = new FastaFormat(int.MaxValue);
      using (StreamReader sr = new StreamReader(options.FastaFile))
      {
        Sequence seq;
        while ((seq = ff.ReadSequence(sr)) != null)
        {
          var name = seq.Name;
          chrNameMap[name] = name;
          chrNameMap[name.StringAfter("chr")] = name;
        }
      }

      var mirnas = new List<BedItem>();
      if (File.Exists(options.MiRBaseFile))
      {
        Progress.SetMessage("Processing {0} ...", options.MiRBaseFile);

        if (options.MiRBaseFile.EndsWith(".bed"))
        {
          mirnas = bedfile.ReadFromFile(options.MiRBaseFile);
          mirnas.ForEach(m =>
          {
            m.Seqname = m.Seqname.StringAfter("chr");
            m.Name = options.MiRBaseKey + ":" + m.Name;
          });
        }
        else
        {
          using (var gf = new GtfItemFile(options.MiRBaseFile))
          {
            GtfItem item;
            while ((item = gf.Next(options.MiRBaseKey)) != null)
            {
              BedItem loc = new BedItem();
              loc.Seqname = item.Seqname.StringAfter("chr");
              loc.Start = item.Start - 1;
              loc.End = item.End;
              loc.Name = options.MiRBaseKey + ":" + item.Attributes.StringAfter("Name=").StringBefore(";");
              loc.Score = 1000;
              loc.Strand = item.Strand;
              mirnas.Add(loc);
            }
          }
        }

        Progress.SetMessage("{0} miRNA readed.", mirnas.Count);
      }

      List<BedItem> trnas = new List<BedItem>();
      if (File.Exists(options.UcscTrnaFile))
      {
        //reading tRNA from ucsc table without mitocondrom tRNA
        Progress.SetMessage("Processing {0} ...", options.UcscTrnaFile);
        trnas = bedfile.ReadFromFile(options.UcscTrnaFile);
        trnas.ForEach(m => m.Seqname = m.Seqname.StringAfter("chr"));

        var removed = trnas.Where(m => (m.Seqname.Length > 1) && !m.Seqname.All(n => char.IsDigit(n))).ToList();
        if (removed.Count != trnas.Count)
        {
          //remove the tRNA not from 1-22, X and Y
          trnas.RemoveAll(m => (m.Seqname.Length > 1) && !m.Seqname.All(n => char.IsDigit(n)));

          //mitocondrom tRNA will be extracted from ensembl gtf file
          trnas.RemoveAll(m => m.Seqname.Equals("M") || m.Seqname.Equals("MT"));
        }

        trnas.ForEach(m => m.Name = GetTRNAName(m.Name));

        Progress.SetMessage("{0} tRNA from ucsc readed.", trnas.Count);

        if (File.Exists(options.UcscMatureTrnaFastaFile))
        {
          var seqs = SequenceUtils.Read(options.UcscMatureTrnaFastaFile);
          foreach (var seq in seqs)
          {
            var tRNAName = GetTRNAName(seq.Name);
            trnas.Add(new BedItem()
            {
              Seqname = seq.Name,
              Start = 0,
              End = seq.SeqString.Length,
              Strand = '+',
              Name = tRNAName,
              Sequence = seq.SeqString
            });
          }
        }
      }

      var others = new List<BedItem>();
      if (File.Exists(options.EnsemblGtfFile))
      {
        //reading smallRNA/tRNA from ensembl gtf file
        Progress.SetMessage("Processing {0} ...", options.EnsemblGtfFile);
        using (var gf = new GtfItemFile(options.EnsemblGtfFile))
        {
          var biotypes = new HashSet<string>(SmallRNAConsts.Biotypes);
          biotypes.Remove(SmallRNAConsts.miRNA);

          GtfItem item;
          int count = 0;
          while ((item = gf.Next("gene")) != null)
          {
            string biotype;
            if (item.Attributes.Contains("gene_biotype"))
            {
              biotype = item.Attributes.StringAfter("gene_biotype \"").StringBefore("\"");
            }
            else if (item.Attributes.Contains("gene_type"))
            {
              biotype = item.Attributes.StringAfter("gene_type \"").StringBefore("\"");
            }
            else
            {
              continue;
            }

            if (File.Exists(options.UcscTrnaFile) && biotype.Equals(SmallRNAConsts.tRNA))
            {
              continue;
            }

            if (biotype.Equals("Mt_tRNA"))
            {
              count++;
              var gene_name = item.Attributes.Contains("gene_name") ? item.Attributes.StringAfter("gene_name \"").StringBefore("\"") : item.GeneId;
              BedItem loc = new BedItem();
              loc.Seqname = "MT";
              loc.Start = item.Start - 1;
              loc.End = item.End;
              loc.Name = string.Format(SmallRNAConsts.tRNA + ":chrMT.tRNA{0}-{1}", count, gene_name.StringAfter("-"));
              loc.Score = 1000;
              loc.Strand = item.Strand;
              trnas.Add(loc);
            }
            else if (biotypes.Contains(biotype))
            {
              string seqName;
              if (item.Seqname.ToLower().StartsWith("chr"))
              {
                seqName = item.Seqname.Substring(3);
              }
              else
              {
                seqName = item.Seqname;
              }
              if (seqName.Equals("M"))
              {
                seqName = "MT";
              }

              //ignore all smallRNA coordinates on scaffold or contig.
              if (seqName.Length > 5)
              {
                continue;
              }

              var gene_name = item.Attributes.StringAfter("gene_name \"").StringBefore("\"");
              var lowGeneName = gene_name.ToLower();
              if (lowGeneName.StartsWith("rny") || lowGeneName.Equals("y_rna"))
              {
                biotype = "yRNA";
              }

              if (lowGeneName.EndsWith("_rrna"))
              {
                biotype = "rRNA";
              }

              BedItem loc = new BedItem();
              loc.Seqname = seqName;
              loc.Start = item.Start - 1;
              loc.End = item.End;
              loc.Name = biotype + ":" + gene_name + ":" + item.GeneId;
              loc.Score = 1000;
              loc.Strand = item.Strand;
              others.Add(loc);
            }
          }
        }
      }

      var all = new List<BedItem>();
      all.AddRange(mirnas);
      all.AddRange(trnas);
      all.AddRange(others);

      foreach (var bi in all)
      {
        if (chrNameMap.ContainsKey(bi.Seqname))
        {
          bi.Seqname = chrNameMap[bi.Seqname];
        }
      }

      if (File.Exists(options.RRNAFile))
      {
        var seqs = SequenceUtils.Read(options.RRNAFile);
        foreach (var seq in seqs)
        {
          all.Add(new BedItem()
          {
            Seqname = seq.Name,
            Start = 0,
            End = seq.SeqString.Length,
            Strand = '+',
            Name = "rRNA:" + SmallRNAConsts.rRNADB_KEY + seq.Name
          });
        }
      }

      Progress.SetMessage("Saving smallRNA coordinates to " + options.OutputFile + "...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        foreach (var pir in SmallRNAConsts.Biotypes)
        {
          var locs = all.Where(m => m.Name.StartsWith(pir)).ToList();
          Progress.SetMessage("{0} : {1}", pir, locs.Count);

          GenomeUtils.SortChromosome(locs, m => m.Seqname, m => (int)m.Start);

          foreach (var loc in locs)
          {
            sw.WriteLine(bedfile.GetValue(loc));
          }
        }
      }

      Progress.SetMessage("Saving smallRNA miss1 coordinates to " + options.OutputFile + ".miss1 ...");
      using (var sw = new StreamWriter(options.OutputFile + ".miss1"))
      {
        foreach (var pir in SmallRNAConsts.Biotypes)
        {
          if (pir == SmallRNABiotype.lincRNA.ToString())
          {
            continue;
          }
          var locs = all.Where(m => m.Name.StartsWith(pir)).ToList();
          locs.RemoveAll(l => l.Name.Contains(SmallRNAConsts.rRNADB_KEY));

          Progress.SetMessage("{0} : {1}", pir, locs.Count);

          GenomeUtils.SortChromosome(locs, m => m.Seqname, m => (int)m.Start);

          foreach (var loc in locs)
          {
            sw.WriteLine(bedfile.GetValue(loc));
          }
        }
      }

      Progress.SetMessage("Saving smallRNA miss1 coordinates to " + options.OutputFile + ".miss0 ...");
      using (var sw = new StreamWriter(options.OutputFile + ".miss0"))
      {
        foreach (var pir in SmallRNAConsts.Biotypes)
        {
          if (pir != SmallRNABiotype.lincRNA.ToString() && pir != SmallRNABiotype.rRNA.ToString())
          {
            continue;
          }
          var locs = all.Where(m => m.Name.StartsWith(pir)).ToList();
          if (pir == SmallRNABiotype.rRNA.ToString())
          {
            locs.RemoveAll(l => !l.Name.Contains(SmallRNAConsts.rRNADB_KEY));
          }

          Progress.SetMessage("{0} : {1}", pir, locs.Count);

          GenomeUtils.SortChromosome(locs, m => m.Seqname, m => (int)m.Start);

          foreach (var loc in locs)
          {
            sw.WriteLine(bedfile.GetValue(loc));
          }
        }
      }

      var summaryFile = options.OutputFile + ".info";
      Progress.SetMessage("Writing summary to " + summaryFile + "...");
      using (var sw = new StreamWriter(summaryFile))
      {
        sw.WriteLine("Biotype\tCount");

        all.ConvertAll(m => m.Name).Distinct().GroupBy(m => m.StringBefore(":")).OrderByDescending(m => m.Count()).ToList().ForEach(m => sw.WriteLine("{0}\t{1}", m.Key, m.Count()));
      }

      var result = new List<string>(new[] { options.OutputFile });

      var fasta = Path.ChangeExtension(options.OutputFile, ".fasta");
      if ((File.Exists(options.UcscTrnaFile) && File.Exists(options.UcscMatureTrnaFastaFile)) || File.Exists(options.RRNAFile))
      {
        result.Add(fasta);
        using (var sw = new StreamWriter(fasta))
        {
          string line;
          using (var sr = new StreamReader(options.FastaFile))
          {
            while ((line = sr.ReadLine()) != null)
            {
              sw.WriteLine(line);
            }
          }

          if (File.Exists(options.UcscTrnaFile) && File.Exists(options.UcscMatureTrnaFastaFile))
          {
            using (var sr = new StreamReader(options.UcscMatureTrnaFastaFile))
            {
              while ((line = sr.ReadLine()) != null)
              {
                sw.WriteLine(line);
              }
            }
          }

          if (File.Exists(options.RRNAFile))
          {
            using (var sr = new StreamReader(options.RRNAFile))
            {
              while ((line = sr.ReadLine()) != null)
              {
                sw.WriteLine(line);
              }
            }
          }
        }
      }

      var faFile = options.OutputFile + ".fa";
      Progress.SetMessage("Extracting sequence from " + options.FastaFile + "...");
      new Bed2FastaProcessor(new Bed2FastaProcessorOptions()
      {
        GenomeFastaFile = options.FastaFile,
        InputFile = options.OutputFile,
        OutputFile = faFile,
        KeepChrInName = false,
        AcceptName = m => m.StartsWith(SmallRNAConsts.miRNA) || m.StartsWith(SmallRNAConsts.tRNA + ":chrMT"),
      })
      {
        Progress = this.Progress
      }.Process();

      if (File.Exists(options.UcscMatureTrnaFastaFile))
      {
        Progress.SetMessage("Extracting sequence from " + options.UcscMatureTrnaFastaFile + " ...");

        using (var sw = new StreamWriter(faFile, true))
        {
          foreach (var tRNA in trnas)
          {
            if (!string.IsNullOrEmpty(tRNA.Sequence))
            {
              sw.WriteLine(">{0}", tRNA.Name);
              sw.WriteLine("{0}", tRNA.Sequence);
            }
          }
        }
      }

      return result;
    }
  }
}
