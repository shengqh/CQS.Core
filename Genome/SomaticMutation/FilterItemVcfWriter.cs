using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CQS.Genome.SomaticMutation
{
  public class FilterItemVcfWriter : IFileWriter<List<FilterItem>>
  {
    private FilterProcessorOptions options;
    public FilterItemVcfWriter(FilterProcessorOptions options)
    {
      this.options = options;
    }

    public void WriteToFile(string fileName, List<FilterItem> t)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.NewLine = "\n";
        sw.WriteLine(@"##fileformat=VCFv4.2
##fileDate={0:yyyyMMdd}
##source={1}
##FILTER=<ID=PASS,Description=""Accept as a confident somatic mutation"">
##FILTER=<ID=GLM_FDR,Description=""Rejected as a confident somatic mutation by FDR adjusted GLM pvalue > {2}"">
##INFO=<ID=BC,Number=0,Type=String,Description=""Is brglm algorithm converaged?"">
##INFO=<ID=BGP,Number=1,Type=Float,Description=""Raw somatic mutation p-value from brglm algorithm"">
##INFO=<ID=BGF,Number=1,Type=Float,Description=""FDR adjusted somatic mutation p-value from brglm algorithm"">
##INFO=<ID=BGSB,Number=1,Type=Float,Description=""Score bias p-value from brglm algorithm"">
##INFO=<ID=BGSTB,Number=1,Type=Float,Description=""Strand bias p-value from brglm algorithm"">
##INFO=<ID=BGPB,Number=1,Type=Float,Description=""Allele position in read bias p-value from brglm algorithm"">
##FORMAT=<ID=GT,Number=1,Type=String,Description=""Genotype"">
##FORMAT=<ID=AD,Number=.,Type=Integer,Description=""Allelic depths for the major/ref and minor/alt alleles in the normal and tumor samples"">
##FORMAT=<ID=FA,Number=A,Type=Float,Description=""Allele fraction of the alternate allele with regard to reference"">", DateTime.Now,
 Path.GetFileNameWithoutExtension(Application.ExecutablePath),
 options.GlmPvalue);

        sw.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO\tFORMAT\tNORMAL\tTUMOR");
        foreach (var item in t)
        {
          sw.WriteLine(GetValue(item));
        }
      }
    }

    public string GetValue(FilterItem item)
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5:0.00}\t{6}\t{7}\t{8}\t{9}\t{10}",
            item.Chr,
            item.Start,
            ".",
            item.MajorAllele,
            item.MinorAllele,
            -Math.Log10(item.BrglmGroupFdr),
            item.Filter,
            GetInformation(item),
            GetFormat(),
            GetFormatValue(item.NormalMajorCount, item.NormalMinorCount),
            GetFormatValue(item.TumorMajorCount, item.TumorMinorCount));
    }

    private string GetGenotype(int major, int minor)
    {
      if (minor <= 1)
      {
        return "0/0";
      }
      else if (major <= 1)
      {
        return "1/1";
      }
      else
      {
        return "0/1";
      }
    }

    public string GetFormat()
    {
      return "GT:AD:FA";
    }

    public string GetFormatValue(int majorAlleleCount, int minorAlleleCount)
    {
      var totalCount = majorAlleleCount + minorAlleleCount;
      var maf = totalCount == 0 ? 0.0 : (minorAlleleCount * 1.0) / totalCount;
      var genoType = GetGenotype(majorAlleleCount, minorAlleleCount);
      return string.Format("{0}:{1},{2}:{3:0.###}", genoType, majorAlleleCount, minorAlleleCount, maf);
    }

    public string GetInformation(FilterItem item)
    {
      var result = new StringBuilder();

      result.AppendFormat("BGP={0:0.#E0};BGF={1:0.#E0}", item.BrglmGroup, item.BrglmGroupFdr);

      if (!string.IsNullOrWhiteSpace(item.BrglmConverged))
      {
        result.AppendFormat(";BC={0}", item.BrglmConverged);
      }

      if (!string.IsNullOrWhiteSpace(item.BrglmScore))
      {
        result.AppendFormat(";BGSB={0:0.#E0}", double.Parse(item.BrglmScore));
      }

      if (!string.IsNullOrWhiteSpace(item.BrglmStrand))
      {
        result.AppendFormat(";BGSTB={0:0.#E0}", double.Parse(item.BrglmStrand));
      }

      if (!string.IsNullOrWhiteSpace(item.BrglmPosition))
      {
        result.AppendFormat(";BGPB={0:0.#E0}", double.Parse(item.BrglmPosition));
      }

      return result.ToString();
    }
  }
}
