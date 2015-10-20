using CQS.Genome.Gtf;
using CQS.Statistics;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Vcf
{
  public class VcfGenotypeTableBuilder : AbstractThreadProcessor
  {
    private VcfGenotypeTableBuilderOptions _options;

    public VcfGenotypeTableBuilder(VcfGenotypeTableBuilderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading " + _options.InputFile + " ...");

      using (var sw = new StreamWriter(_options.OutputFile))
      {
        using (var sr = new StreamReader(_options.InputFile))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith("##"))
            {
              continue;
            }
            else
            {
              break;
            }
          }

          var headerparts = line.Split('\t');
          var formatIndex = Array.IndexOf(headerparts, "FORMAT");
          sw.WriteLine("{0}\t{1}\t{2}\tConsistent\tConsistentPercentage",
            headerparts.Take(formatIndex).Merge('\t'),
            (from hr in headerparts.Skip(formatIndex + 1)
             select "GenoType_" + hr).Merge('\t'),
             (from hr in headerparts.Skip(formatIndex + 1)
              select "AlleleDepth_" + hr).Merge('\t'));

          int gtindex = -2;
          int adindex = -2;
          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t').ToArray();
            if (parts.Length < formatIndex)
            {
              break;
            }

            if (gtindex == -2)
            {
              gtindex = parts[formatIndex].Split(':').ToList().FindIndex(m => m.Equals("GT"));
            }

            if (adindex == -2)
            {
              adindex = parts[formatIndex].Split(':').ToList().FindIndex(m => m.Equals("AD"));
            }

            var data = parts.Skip(formatIndex + 1).ToList();
            if (data.Any(l => GetAlterAlleles(l, adindex).Split(',').Skip(1).Any(m => int.Parse(m) >= _options.MinimumDepth)))
            {
              var sampleData = parts.Skip(formatIndex + 1).ToArray();
              var genotypes = (from part in sampleData
                               select StringToGenoType(part, gtindex)).ToList();
              var alleles = (from part in sampleData
                             select GetAlterAlleles(part, adindex)).ToList();

              if (_options.RecallGenotype)
              {
                for (int i = 0; i < alleles.Count; i++)
                {
                  var refAllele = int.Parse(alleles[i].StringBefore(","));
                  var altAllele = int.Parse(alleles[i].StringAfter(",").StringBefore(","));

                  if (string.IsNullOrEmpty(genotypes[i]))
                  {
                    if (altAllele == 0 && refAllele > 2)
                    {
                      genotypes[i] = "0";
                    }
                    continue;
                  }

                  if (altAllele == 0)
                  {
                    genotypes[i] = "0";
                  }
                  else if (refAllele == 0)
                  {
                    genotypes[i] = "2";
                  }
                  else if (refAllele == 1 || altAllele == 1)
                  {
                    var old = genotypes[i];
                    RecalibrateGenotype(genotypes, i, refAllele, altAllele);
                    if (old != genotypes[i])
                    {
                      Console.WriteLine("Ref={0}, Alt={1}, Old={2}, New={3}", refAllele, altAllele, old, genotypes[i]);
                    }
                  }
                }
              }

              sw.Write("{0}\t{1}\t{2}", parts.Take(formatIndex).Merge("\t"), genotypes.Merge("\t"), alleles.Merge("\t"));

              var gp = genotypes.Where(m => !string.IsNullOrEmpty(m)).GroupBy(m => m).OrderByDescending(m => m.Count()).ToList();
              var allValid = (from g in gp select g.Count()).Sum();
              if (gp.Count == 1)
              {
                sw.WriteLine("\t{0}|{0}\t100", allValid);
              }
              else
              {
                sw.WriteLine("\t{0}|{1}\t{2:0.##}", gp[0].Count(), allValid, gp[0].Count() * 100.0 / allValid);
              }
            }
          }
        }
      }

      return new string[] { _options.OutputFile };
    }

    private static void RecalibrateGenotype(List<string> genotypes, int index, int refAllele, int altAllele)
    {
      if (refAllele == 1)
      {
        genotypes[index] = altAllele > 4 ? "2" : "1";
      }
      else
      {
        genotypes[index] = refAllele > 4 ? "0" : "1";
      }
    }

    private static void RecalibrateGenotypeByFisherExactTest(List<string> genotypes, int index, int refAllele, int altAllele)
    {
      var totalAllele = refAllele + altAllele;
      var halfAllele1 = totalAllele / 2;
      var halfAllele2 = totalAllele - halfAllele1;

      var g0 = MyFisherExactTest.TwoTailPValue(refAllele, altAllele, totalAllele, 0);
      var g1 = MyFisherExactTest.TwoTailPValue(refAllele, altAllele, halfAllele1, halfAllele2);
      if (halfAllele1 != halfAllele2)
      {
        g1 = Math.Max(g1, MyFisherExactTest.TwoTailPValue(refAllele, altAllele, halfAllele2, halfAllele1));
      }
      var g2 = MyFisherExactTest.TwoTailPValue(refAllele, altAllele, 0, totalAllele);

      if (g0 > g1 && g0 > g2)
      {
        genotypes[index] = "0";
      }
      else if (g1 > g2)
      {
        genotypes[index] = "1";
      }
      else
      {
        genotypes[index] = "2";
      }
    }

    private static string GetAlterAlleles(string l, int adindex)
    {
      var parts = l.Split(':');
      if (parts.Length > adindex)
      {
        return parts[adindex];
      }
      else
      {
        return "0,0";
      }
    }

    private string StringToGenoType(string part, int gtindex)
    {
      var parts = part.Split(':');
      var gt = parts[gtindex];
      if (gt.StartsWith("0/1"))
      {
        return "1";
      }
      else if (gt.StartsWith("1/1"))
      {
        return "2";
      }
      else if (gt.StartsWith("./."))
      {
        return "";
      }
      else
      {
        return "0";
      }
    }
  }
}

