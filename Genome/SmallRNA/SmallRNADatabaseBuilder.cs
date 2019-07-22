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
using System.Text.RegularExpressions;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNADatabaseBuilder : AbstractThreadProcessor
  {

    private SmallRNADatabaseBuilderOptions options;

    public SmallRNADatabaseBuilder(SmallRNADatabaseBuilderOptions options)
    {
      this.options = options;
    }

    public static string GetTRNAName(string oldName)
    {
      var result = oldName;
      while (result.Contains("_"))
      {
        var substring = result.StringAfter("_");
        if (substring.ToLower().Contains("trna"))
        {
          result = substring;
        }
        else
        {
          break;
        }
      }

      //result = Regex.Replace(result, "\\(.+?\\)", ""); 

      result = "tRNA:" + result;
      if (result.Contains("chrM"))
      {
        result = "mt_" + result;
      }
      return (result);
    }

    public override IEnumerable<string> Process()
    {
      var paramFile = options.OutputFile + ".param";
      options.SaveToFile(options.OutputFile + ".param");

      var bedfile = new BedItemFile<BedItem>(6);

      Progress.SetMessage("building chromosome name map ...");

      var mitoName = "M";
      Dictionary<string, string> chrNameMap = new Dictionary<string, string>();
      var ff = new FastaFormat(int.MaxValue);

      var faiFile = options.FastaFile + ".fai";
      if (File.Exists(faiFile))
      {
        using (StreamReader sr = new StreamReader(faiFile))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            var name = line.Split('\t')[0];
            chrNameMap[name] = name;
            if (name.StartsWith("chr"))
            {
              chrNameMap[name.StringAfter("chr")] = name;
            }
            if (!name.StartsWith("chr"))
            {
              chrNameMap["chr" + name] = name;
            }

            if (name.Equals("chrMT") || name.Equals("MT"))
            {
              mitoName = "MT";
            }
            if (name.Equals("chrM") || name.Equals("M"))
            {
              mitoName = "M";
            }
          }
        }
      }
      else
      {
        using (StreamReader sr = new StreamReader(options.FastaFile))
        {
          Sequence seq;
          while ((seq = ff.ReadSequence(sr)) != null)
          {
            var name = seq.Name;
            chrNameMap[name] = name;
            if (name.StartsWith("chr"))
            {
              chrNameMap[name.StringAfter("chr")] = name;
            }
            if (!name.StartsWith("chr"))
            {
              chrNameMap["chr" + name] = name;
            }

            if (name.Equals("chrMT") || name.Equals("MT"))
            {
              mitoName = "MT";
            }
            if (name.Equals("chrM") || name.Equals("M"))
            {
              mitoName = "M";
            }
          }
        }
      }
      var longMitoName = chrNameMap[mitoName];
      Progress.SetMessage("mitochondral chromosome name = {0}", longMitoName);

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
              loc.Seqname = mitoName;
              loc.Start = item.Start - 1;
              loc.End = item.End;
              loc.Name = string.Format(SmallRNAConsts.mt_tRNA + ":" + longMitoName + ".tRNA{0}-{1}", count, gene_name.StringAfter("-"));
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
              if (seqName.Equals("M") || seqName.Equals("MT"))
              {
                seqName = mitoName;
              }

              //ignore all smallRNA coordinates on scaffold or contig.
              //if (seqName.Length > 5)
              //{
              //  continue;
              //}

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

      var miRNA_bed = FileUtils.ChangeExtension(options.OutputFile, ".miRNA.bed");
      Progress.SetMessage("Saving miRNA coordinates to " + miRNA_bed + "...");
      using (var sw = new StreamWriter(miRNA_bed))
      {
        var pir = SmallRNAConsts.miRNA;
        var locs = all.Where(m => m.Name.StartsWith(pir)).ToList();
        Progress.SetMessage("{0} : {1}", pir, locs.Count);

        GenomeUtils.SortChromosome(locs, m => m.Seqname, m => (int)m.Start);

        foreach (var loc in locs)
        {
          sw.WriteLine(bedfile.GetValue(loc));
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
      var b2foptions = new Bed2FastaProcessorOptions()
      {
        GenomeFastaFile = options.FastaFile,
        InputFile = options.OutputFile,
        OutputFile = faFile,
        KeepChrInName = false,
      };

      if (!File.Exists(options.UcscMatureTrnaFastaFile))
      {
        b2foptions.AcceptName = m => m.StartsWith(SmallRNAConsts.miRNA) || m.StartsWith(SmallRNAConsts.mt_tRNA) || m.StartsWith(SmallRNAConsts.tRNA);
      }
      else
      {
        b2foptions.AcceptName = m => m.StartsWith(SmallRNAConsts.miRNA) || m.StartsWith(SmallRNAConsts.mt_tRNA);
      }

      new Bed2FastaProcessor(b2foptions)
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
