using RCPA;
using RCPA.Gui;
using RCPA.Seq;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.Gwas
{
  public static class SNPItemUtils
  {
    public static int HumanChromosomeToInt(string chromosome)
    {
      var chr = chromosome.ToUpper().StringAfter("CHR");

      int result;
      if (int.TryParse(chr, out result))
      {
        return result;
      }

      if (chr.Equals("X"))
      {
        return 23;
      }

      if (chr.Equals("Y"))
      {
        return 24;
      }

      if (chr.Equals("M") || chr.Equals("MT"))
      {
        return 26;
      }

      throw new Exception("Unknown chromosome " + chromosome);
    }

    /// <summary>
    /// Fill reference allele from genome fasta file.
    /// </summary>
    /// <param name="snpItems"></param>
    /// <param name="fastaFile"></param>
    /// <param name="progress"></param>
    public static void FillReferenceAlleleFromFasta(this IEnumerable<SNPItem> snpItems, string fastaFile, IProgressCallback progress = null)
    {
      if (progress == null)
      {
        progress = new ConsoleProgressCallback();
      }

      var dic = snpItems.ToGroupDictionary(m => m.Chrom);

      progress.SetMessage("Filling reference allele from {0} file ...", fastaFile);
      using (var sw = new StreamReader(fastaFile))
      {
        var ff = new FastaFormat();
        Sequence seq;
        while ((seq = ff.ReadSequence(sw)) != null)
        {
          progress.SetMessage("chromosome " + seq.Name + " ...");
          var chr = HumanChromosomeToInt(seq.Name);
          if (dic.ContainsKey(chr))
          {
            var snps = dic[chr];
            foreach (var snp in snps)
            {
              snp.RefChar = char.ToUpper(seq.SeqString[snp.Position - 1]);
            }
          }
        }
      }
      progress.SetMessage("Filling reference allele finished.");
    }

    /// <summary>
    /// Fill dbsnp information. The name of SNPItem will be replaced by dbSNP name and the mapping between dbSNP name and old SNPItem name will be returned.
    /// </summary>
    /// <param name="snpItems"></param>
    /// <param name="dbSnpVcfFile"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public static Dictionary<string, string> FillDbsnpIdByPosition(this IEnumerable<SNPItem> snpItems, string dbSnpVcfFile, IProgressCallback progress = null)
    {
      var sourceDbsnpMap = snpItems.ToDictionary(m => m.Name, m => m.Name);

      if (progress == null)
      {
        progress = new  EmptyProgressCallback();
      }

      var dic = snpItems.ToDoubleDictionary(m => m.Chrom, m => m.Position);

      progress.SetMessage("Filling dbSNP id from {0} ...", dbSnpVcfFile);
      using (var sr = new StreamReader(dbSnpVcfFile))
      {
        progress.SetRange(0, sr.BaseStream.Length);

        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith("##"))
          {
            break;
          }
        }

        int linecount = 0;
        Dictionary<int, SNPItem> chrMap = null;
        int lastChr = -1;
        while (line != null)
        {
          linecount++;

          if (linecount % 10000 == 0)
          {
            progress.SetPosition(sr.GetCharpos());
          }

          try
          {
            //make sure it is SNV
            if (!line.Contains("VC=SNV"))
            {
              continue;
            }

            //Even it marked as SNV, it still could be insertion/deletion
            //2       179658175       rs11537855      C       CC,CT   .       .       RS=11537855;RSPOS=179658175;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050100001205000002000110;GENEINFO=TTN:7273;WGT=1;VC=SNV;SLO;NSF;REF;ASP;OTHERKG;NOC
            var parts = line.Split('\t');
            if (parts[3].Split(',').Any(l => l.Length != 1))
            {
              continue;
            }

            if (parts[4].Split(',').Any(l => l.Length != 1))
            {
              continue;
            }

            var chr = HumanChromosomeToInt(parts[0]);
            var position = int.Parse(parts[1]);

            if (lastChr != chr)
            {
              if (!dic.TryGetValue(chr, out chrMap))
              {
                continue;
              }
              lastChr = chr;
            }

            SNPItem source;
            if (!chrMap.TryGetValue(position, out source))
            {
              continue;
            }

            if (!source.Name.Equals(parts[2]))
            {
              sourceDbsnpMap.Remove(source.Name);
              sourceDbsnpMap[source.Name] = parts[2];
            }

            source.DbsnpRefAllele = parts[3][0];
            source.DbsnpAltAllele = parts[4][0];
            source.DbsnpIsReversed = parts[7].Contains(";RV;");
          }
          finally
          {
            line = sr.ReadLine();
          }
        }
      }

      var snpMap = snpItems.ToDictionary(m => m.Name);
      var result = new Dictionary<string, string>();
      foreach (var r in sourceDbsnpMap)
      {
        result[r.Value] = r.Key;
        if (!r.Key.Equals(r.Value))
        {
          snpMap[r.Key].Name = r.Value;
        }
      }

      progress.SetMessage("Filling dbSNP id finished.");
      return result;
    }

    private static void DoFillAllele2FrequencyFrom1000Gome(this IEnumerable<SNPItem> snpItems, string g1000VcfFile, Func<SNPItem, string> keyFunc, IProgressCallback progress = null)
    {
      if (progress == null)
      {
        progress = new ConsoleProgressCallback();
      }

      var dic = snpItems.ToDictionary(m => keyFunc(m));

      progress.SetMessage("Filling MAF from {0} ...", g1000VcfFile);
      using (var sr = new StreamReader(g1000VcfFile))
      {
        progress.SetRange(0, sr.BaseStream.Length);

        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith("##"))
          {
            break;
          }
        }

        int linecount = 0;
        while ((line = sr.ReadLine()) != null)
        {
          linecount++;

          if (linecount % 10000 == 0)
          {
            progress.SetPosition(sr.GetCharpos());
          }

          var parts = line.Split('\t');
          var snp = new SNPItem()
          {
            Chrom = HumanChromosomeToInt(parts[0]),
            Position = int.Parse(parts[1]),
            Name = parts[2]
          };

          SNPItem loc;
          if (!dic.TryGetValue(keyFunc(snp), out loc))
          {
            continue;
          }

          loc.G1000Allele1 = parts[3][0];
          var allele2 = parts[4].Split(',');
          var frequencies = parts[7].StringAfter("AF=").StringBefore(";").Split(',');
          bool bFound = false;
          for (int i = 0; i < allele2.Length; i++)
          {
            if (allele2[i].Length != 1)
            {
              continue;
            }

            loc.G1000Allele2 = allele2[i][0];
            if (loc.IsSourceAllelesMatchedWithG1000())
            {
              loc.G1000Allele2Frequency = double.Parse(frequencies[i]);
              bFound = true;
              break;
            }
          }

          if (!bFound)
          {
            loc.G1000Allele1 = ' ';
            loc.G1000Allele2 = ' ';
            loc.G1000Allele2Frequency = 0.0;
          }
        }
      }
      progress.SetMessage("Filling MAF finished.");
    }

    public static void FindAllele2FrequencyFrom1000GomeByLocus(this IEnumerable<SNPItem> snpItems, string g1000VcfFile, IProgressCallback progress = null)
    {
      DoFillAllele2FrequencyFrom1000Gome(snpItems, g1000VcfFile, m => string.Format("{0}:{1}", m.Chrom, m.Position), progress);
    }

    public static void FindAllele2FrequencyFrom1000GomeByName(this IEnumerable<SNPItem> snpItems, string g1000VcfFile, IProgressCallback progress = null)
    {
      DoFillAllele2FrequencyFrom1000Gome(snpItems, g1000VcfFile, m => m.Name, progress);
    }
  }
}
