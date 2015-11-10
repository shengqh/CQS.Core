using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.SomaticMutation
{
  public static class SomaticMutationUtils
  {
    public static readonly Regex MutectPattern = new Regex(@"[^:]+:(\d+),(\d+)");

    public static readonly Regex Varscan2Pattern = new Regex(@"^.+?:.+?:\d+:(\d+):(\d+)");

    public static Dictionary<string, Dictionary<string, SomaticItem>> ParseAnnovarDirectory(string rootdir, Regex reg, string scorePrefix, Func<string, bool> acceptChromosome)
    {
      return (from file in Directory.GetFiles(rootdir, "*.final.tsv", SearchOption.AllDirectories)
              from si in ParseAnnovarFile(file, reg, scorePrefix, acceptChromosome)
              select si).GroupBy(m => m.Sample).ToDictionary(m => m.Key, m => m.ToDictionary(n => n.Key));
    }

    public static Dictionary<string, Dictionary<string, SomaticItem>> ParseMutectDirectory(string rootdir, Func<string, bool> acceptChromosome)
    {
      return (from file in Directory.GetFiles(rootdir, "*.final.tsv", SearchOption.AllDirectories)
              from si in ParseAnnovarMutectFile(file, acceptChromosome)
              select si).GroupBy(m => m.Sample).ToDictionary(m => m.Key, m => m.ToDictionary(n => n.Key));
    }

    public static Dictionary<string, Dictionary<string, SomaticItem>> ParseVarscan2Directory(string rootdir, Func<string, bool> acceptChromosome)
    {
      return (from file in Directory.GetFiles(rootdir, "*.final.tsv", SearchOption.AllDirectories)
              from si in ParseAnnovarVarscan2File(file, acceptChromosome)
              select si).GroupBy(m => m.Sample).ToDictionary(m => m.Key, m => m.ToDictionary(n => n.Key));
    }

    public static Dictionary<string, Dictionary<string, SomaticItem>> ParseGlmvcDirectory(string resultDir, Func<string, bool> acceptChromosome)
    {
      var files = Directory.GetFiles(resultDir, "*.annotation.tsv", SearchOption.AllDirectories);

      return (from file in files
              from item in ParseGlmvcFile(file, acceptChromosome)
              select item).GroupBy(m => m.Sample).ToDictionary(m => m.Key, m => m.ToDictionary(n => n.Key));
    }

    public static List<SomaticItem> ParseAnnovarMutectFile(string fileName, Func<string, bool> acceptChromosome)
    {
      return ParseAnnovarFile(fileName, MutectPattern, "LOD=", acceptChromosome);
    }

    public static List<SomaticItem> ParseAnnovarVarscan2File(string fileName, Func<string, bool> acceptChromosome)
    {
      var result = ParseAnnovarFile(fileName, Varscan2Pattern, "SPV=", acceptChromosome);
      result.ForEach(l => l.Score = -Math.Log(l.Score));
      return result;
    }

    public static List<SomaticItem> ParseAnnovarFile(string fileName, Regex reg, string scorePrefix, Func<string, bool> acceptChromosome)
    {
      var result = new List<SomaticItem>();

      var bar = Path.GetFileName(fileName).StringBefore(".");
      var annos = new AnnotationFormat("^#").ReadFromFile(fileName);
      var headers = File.ReadAllLines(fileName).Where(l => !l.StartsWith("#")).First().Split('\t');
      foreach (var ann in annos)
      {
        //Chr	Start	End	Ref	Alt	Func.refGene	Gene.refGene	GeneDetail.refGene	ExonicFunc.refGene	AAChange.refGene	snp138	cosmic70	FILTER	INFO	FORMAT	H_LS-A7-A0D9-01A-31W-A071-09-1	H_LS-A7-A0D9-10A-01W-A071-09-1

        var chr = ann.Annotations["Chr"].ToString();
        if (!acceptChromosome(chr))
        {
          continue;
        }

        var m1 = reg.Match(ann.Annotations[headers[headers.Length - 2]].ToString());
        var m1Major = int.Parse(m1.Groups[1].Value);
        var m1Minor = int.Parse(m1.Groups[2].Value);
        var m2 = reg.Match(ann.Annotations[headers[headers.Length - 1]].ToString());
        var m2Major = int.Parse(m2.Groups[1].Value);
        var m2Minor = int.Parse(m2.Groups[2].Value);
        var isNormalFirst = (((double)m1Minor) / (m1Major + m1Minor)) < (((double)m2Minor) / (m2Major + m2Minor));

        var info = ann.Annotations["INFO"].ToString();

        var item = new SomaticItem()
        {
          Sample = bar,
          Chrom = chr,
          StartPosition = int.Parse(ann.Annotations["Start"].ToString()),
          RefAllele = ann.Annotations["Ref"].ToString(),
          AltAllele = ann.Annotations["Alt"].ToString(),
          NormalMajorCount = isNormalFirst ? m1Major : m2Major,
          NormalMinorCount = isNormalFirst ? m1Minor : m2Minor,
          TumorMajorCount = isNormalFirst ? m2Major : m1Major,
          TumorMinorCount = isNormalFirst ? m2Minor : m1Minor,
          Score = double.Parse(info.StringAfter(scorePrefix)),
          RefGeneFunc = GetDictionaryValue(ann.Annotations, "Func.refGene", string.Empty),
          RefGeneName = GetDictionaryValue(ann.Annotations, "Gene.refGene", string.Empty),
          RefGeneExonicFunc = GetDictionaryValue(ann.Annotations, "ExonicFunc.refGene", string.Empty)
        };

        result.Add(item);
      }
      return result;
    }

    public static List<SomaticItem> ParseGlmvcFile(string fileName, Func<string, bool> acceptChromosome)
    {
      var result = new List<SomaticItem>();

      var bar = Path.GetFileName(fileName).StringBefore(".");
      var annos = new AnnotationFormat().ReadFromFile(fileName);
      foreach (var ann in annos)
      {
        var chr = ann.Annotations["chr"].ToString();
        if (!acceptChromosome(chr))
        {
          continue;
        }

        var fdr = ann.Annotations["brglm_group_fdr"].ToString();
        var item = new SomaticItem()
        {
          Sample = bar,
          Chrom = chr,
          StartPosition = int.Parse(ann.Annotations["start"].ToString()),
          RefAllele = ann.Annotations["major_allele"].ToString(),
          AltAllele = ann.Annotations["minor_allele"].ToString(),
          NormalMajorCount = int.Parse(ann.Annotations["normal_major_count"].ToString()),
          NormalMinorCount = int.Parse(ann.Annotations["normal_minor_count"].ToString()),
          TumorMajorCount = int.Parse(ann.Annotations["tumor_major_count"].ToString()),
          TumorMinorCount = int.Parse(ann.Annotations["tumor_minor_count"].ToString()),
          LogisticScore = ann.Annotations["brglm_score"].ToString(),
          LogisticStrand = ann.Annotations["brglm_strand"].ToString(),
          LogisticPosition = ann.Annotations["brglm_position"].ToString(),
          LogisticGroupFdr = fdr,
          Score = -Math.Log(double.Parse(fdr)),
          RefGeneFunc = GetDictionaryValue(ann.Annotations, "annovar_Func.refGene", string.Empty),
          RefGeneName = GetDictionaryValue(ann.Annotations, "annovar_Gene.refGene", string.Empty),
          RefGeneExonicFunc = GetDictionaryValue(ann.Annotations, "annovar_ExonicFunc.refGene", string.Empty)
        };

        result.Add(item);
      }
      return result;
    }

    private static string GetDictionaryValue(Dictionary<string, object> dic, string key, string defaultValue)
    {
      object value;
      if (dic.TryGetValue(key, out value))
      {
        return value.ToString();
      }
      else
      {
        return defaultValue;
      }
    }
  }
}