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

    public override IEnumerable<string> Process()
    {
      var paramFile = options.OutputFile + ".param";
      if (string.IsNullOrEmpty(options.ParamFile) || !Path.GetFullPath(options.ParamFile).Equals(Path.GetFullPath(paramFile)))
      {
        options.SaveToFile(options.OutputFile + ".param");
      }

      var bedfile = new BedItemFile<BedItem>();

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

        Progress.SetMessage("{0} miRNA read.", mirnas.Count);
      }

      List<BedItem> trnas = new List<BedItem>();

      if (File.Exists(options.UcscTrnaFile))
      {
        //reading tRNA from ucsc table without mitocondrom tRNA
        Progress.SetMessage("Processing {0} ...", options.UcscTrnaFile);
        trnas = bedfile.ReadFromFile(options.UcscTrnaFile);
        trnas.ForEach(m => m.Seqname = m.Seqname.StringAfter("chr"));

        //remove the tRNA not from 1-22, X and Y
        trnas.RemoveAll(m => (m.Seqname.Length > 1) && !m.Seqname.All(n => char.IsDigit(n)));

        //mitocondrom tRNA will be extracted from ensembl gtf file
        trnas.RemoveAll(m => m.Seqname.Equals("M") || m.Seqname.Equals("MT"));

        trnas.ForEach(m => m.Name = SmallRNAConsts.tRNA + ":" + m.Name);

        Progress.SetMessage("{0} tRNA from ucsc read.", trnas.Count);
      }

      //reading smallRNA/tRNA from ensembl gtf file
      Progress.SetMessage("Processing {0} ...", options.EnsemblGtfFile);
      var others = new List<BedItem>();
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
            var gene_name = item.Attributes.StringAfter("gene_name \"").StringBefore("\"");
            BedItem loc = new BedItem();
            loc.Seqname = item.Seqname.StringAfter("chr");
            if (loc.Seqname.Equals("M"))
            {
              loc.Seqname = "MT";
            }
            loc.Start = item.Start - 1;
            loc.End = item.End;
            loc.Name = biotype + ":" + gene_name + ":" + item.GeneId;
            loc.Score = 1000;
            loc.Strand = item.Strand;
            others.Add(loc);
          }
        }
      }

      var all = new List<BedItem>();
      all.AddRange(mirnas);
      all.AddRange(trnas);
      all.AddRange(others);

      Progress.SetMessage("Saving smallRNA coordinates to " + options.OutputFile + "...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        foreach (var pir in SmallRNAConsts.Biotypes)
        {
          var locs = all.Where(m => m.Name.StartsWith(pir)).ToList();

          GenomeUtils.SortChromosome(locs, m => m.Seqname, m => (int)m.Start);

          foreach (var loc in locs)
          {
            sw.WriteLine(bedfile.GetValue(loc));
          }
        }
      }

      Progress.SetMessage("Extracting sequence from " + options.FastaFile + "...");
      new Bed2FastaProcessor(new Bed2FastaProcessorOptions()
      {
        GenomeFastaFile = options.FastaFile,
        InputFile = options.OutputFile,
        OutputFile = options.OutputFile + ".fa",
        KeepChrInName = false,
        AcceptName = m => m.StartsWith(SmallRNAConsts.miRNA) || m.StartsWith(SmallRNAConsts.tRNA),
      })
      {
        Progress = this.Progress
      }.Process();

      var summaryFile = options.OutputFile + ".info";
      Progress.SetMessage("Writing summary to " + summaryFile + "...");
      using (var sw = new StreamWriter(summaryFile))
      {
        sw.WriteLine("Biotype\tCount");

        all.ConvertAll(m => m.Name).Distinct().GroupBy(m => m.StringBefore(":")).OrderByDescending(m => m.Count()).ToList().ForEach(m => sw.WriteLine("{0}\t{1}", m.Key, m.Count()));
      }

      return new string[] { options.OutputFile };
    }
  }
}
