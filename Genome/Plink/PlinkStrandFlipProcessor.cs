using CQS.Genome.Bed;
using CQS.Genome.Gwas;
using CQS.Genome.Plink;
using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.Plink
{
  public class PlinkStrandFlipProcessor : AbstractThreadProcessor
  {
    private PlinkStrandFlipProcessorOptions options;

    public PlinkStrandFlipProcessor(PlinkStrandFlipProcessorOptions options)
    {
      this.options = options;
    }

    private static bool IsMissing(PlinkLocus m)
    {
      return m.Allele1.Equals("0") && m.Allele2.Equals("0");
    }

    private static bool IsIndel(PlinkLocus m)
    {
      return m.Allele1.Length != 1 || m.Allele2.Length != 1 || m.Allele1.Equals("I") || m.Allele1.Equals("D") || m.Allele2.Equals("I") || m.Allele2.Equals("D");
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var bimfile = Path.ChangeExtension(options.InputFile, ".bim");

      var snps = PlinkLocus.ReadFromBimFile(bimfile, false, false);
      snps.RemoveAll(m => IsIndel(m) || IsMissing(m));

      var snpItems = (from snp in snps
                      select new SNPItem()
                      {
                        Chrom = snp.Chromosome,
                        Name = snp.MarkerId,
                        Position = snp.PhysicalPosition,
                        Allele1 = snp.Allele1[0],
                        Allele2 = snp.Allele2
                      }).ToList();

      var nameMap = snpItems.FillDbsnpIdByPosition(options.DbsnpFile, this.Progress);

      using (var sw = new StreamWriter(options.OutputPrefix + ".namemap"))
      {
        sw.WriteLine("NewName\tOldName");
        foreach (var n in nameMap)
        {
          sw.WriteLine("{0}\t{1}", n.Key, n.Value);
        }
      }

      //remove all snps without corresponding dbsnp entry
      snpItems.RemoveAll(m => m.DbsnpRefAllele == ' ');

      var nameDic = snpItems.ToGroupDictionary(m => m.Name);
      foreach (var n in nameDic)
      {
        if (n.Value.Count > 1)
        {
          Console.Error.WriteLine("Duplicated SNP:" + n.Key);
          foreach (var v in n.Value)
          {
            Console.Error.WriteLine("{0}:{1}-{2}:{3},{4}:{5},{6}", n.Key, v.Chrom, v.Position, v.Allele1, v.Allele2, v.DbsnpRefAllele, v.DbsnpAltAllele);
          }
        }
      }

      if (File.Exists(options.G1000File))
      {
        snpItems.FindAllele2FrequencyFrom1000GomeByName(options.G1000File, this.Progress);
      }

      if (File.Exists(options.FastaFile))
      {
        snpItems.FillReferenceAlleleFromFasta(options.FastaFile, this.Progress);
      }

      Dictionary<string, StrandAction> actionMap = new Dictionary<string, StrandAction>();
      
      var statFile = options.OutputPrefix + ".stat";
      result.Add(statFile);
      using (var sw = new StreamWriter(statFile))
      {
        sw.WriteLine("Name\tChromosome\tPosition\tSource_Allele1\tSource_Allele2\tReference_Allele\tDbsnp_RefAllele\tDbsnp_AltAllele\tDbsnp_IsReversed\tG1000_RefAllele\tG1000_AltAllele\tG1000_MAF\tAction");

        foreach (var v in snpItems)
        {
          StrandAction action = v.SuggestAction();
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:0.####}\t{12}", v.Name, v.Chrom, v.Position, v.Allele1, v.Allele2, v.RefChar, v.DbsnpRefAllele, v.DbsnpAltAllele, v.DbsnpIsReversed, v.G1000Allele1, v.G1000Allele2, v.G1000Allele2Frequency, action);
          actionMap[v.Name] = action;
        }
      }

      using (var reader = new PlinkBedRandomFile(options.InputFile) { Progress = this.Progress })
      {
        var data = reader.Data;

        var chrs = (from v in snpItems select v.Chrom).Distinct().OrderBy(m => m).ToArray();
        foreach (var chr in chrs)
        {
          var genfile = string.Format("{0}.{1}.gen", options.OutputPrefix, chr.ToString().PadLeft(2, '0'));
          result.Add(genfile);
          var map = FileUtils.ChangeExtension(genfile, ".sample");

          new GwasSampleFormat().WriteToFile(map, data.Individual);

          //save gen file
          using (var sw = new StreamWriter(genfile))
          {
            sw.NewLine = Environment.NewLine;
            var chrItems = snpItems.Where(m => m.Chrom == chr).ToList();
            GenomeUtils.SortChromosome(chrItems, m => chr.ToString(), m => m.Position);
            foreach (var snp in chrItems)
            {
              var ldata = reader.Read(nameMap[snp.Name]);
              var action = actionMap[snp.Name];

              sw.Write("{0} {1} {2} {3} {4}", snp.Chrom, snp.Name, snp.Position, snp.DbsnpRefAllele, snp.DbsnpAltAllele);
              for (int individualIndex = 0; individualIndex < data.Individual.Count; individualIndex++)
              {

                if (PlinkData.IsMissing(ldata[0, individualIndex], ldata[1, individualIndex]))
                {
                  sw.Write(" 0 0 0");
                }
                else
                {
                  char alle1, alle2;
                  if (StrandAction.Switch == action || StrandAction.FlipSwitch == action)
                  {
                    alle1 = ldata[0, individualIndex] ? snp.DbsnpAltAllele : snp.DbsnpRefAllele;
                    alle2 = ldata[1, individualIndex] ? snp.DbsnpAltAllele : snp.DbsnpRefAllele;
                  }
                  else
                  {
                    alle1 = ldata[0, individualIndex] ? snp.DbsnpRefAllele : snp.DbsnpAltAllele;
                    alle2 = ldata[1, individualIndex] ? snp.DbsnpRefAllele : snp.DbsnpAltAllele;
                  }

                  if (alle1 != alle2)
                  {
                    sw.Write(" 0 1 0");
                  }
                  else if (alle1 == snp.DbsnpRefAllele)
                  {
                    sw.Write(" 1 0 0");
                  }
                  else
                  {
                    sw.Write(" 0 0 1");
                  }
                }
              }
              sw.WriteLine();
            }
          }
        }
      }

      return result;
    }
  }
}