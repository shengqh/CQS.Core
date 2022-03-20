using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.SomaticMutation
{
  public class FilterItem
  {
    public string Chr { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string MajorAllele { get; set; }
    public string MinorAllele { get; set; }
    public string ReferenceAllele { get; set; }
    public int NormalMajorCount { get; set; }
    public int NormalMinorCount { get; set; }
    public int TumorMajorCount { get; set; }
    public int TumorMinorCount { get; set; }
    public double FisherGroup { get; set; }
    public string FisherNormal { get; set; }
    public string BrglmConverged { get; set; }
    public double BrglmGroup { get; set; }
    public double BrglmGroupFdr { get; set; }
    public string BrglmScore { get; set; }
    public string BrglmStrand { get; set; }
    public string BrglmPosition { get; set; }
    public string Filter { get; set; }
    public string Identity { get; set; }
  }

  public class FilterItemTextFormat : IFileFormat<List<FilterItem>>
  {
    public List<FilterItem> ReadFromFile(string fileName)
    {
      var anns = new AnnotationFormat().ReadFromFile(fileName);
      var result = new List<FilterItem>();
      foreach (var ann in anns)
      {
        var item = new FilterItem();
        item.Chr = ann.Annotations["chr"] as string;
        item.Start = ann.Annotations["start"] as string;
        item.End = ann.Annotations["end"] as string;
        item.MajorAllele = ann.Annotations["major_allele"] as string;
        item.MinorAllele = ann.Annotations["minor_allele"] as string;
        item.ReferenceAllele = ann.Annotations["ref_allele"] as string;
        item.NormalMajorCount = int.Parse(ann.Annotations["normal_major_count"] as string);
        item.NormalMinorCount = int.Parse(ann.Annotations["normal_minor_count"] as string);
        item.TumorMajorCount = int.Parse(ann.Annotations["tumor_major_count"] as string);
        item.TumorMinorCount = int.Parse(ann.Annotations["tumor_minor_count"] as string);
        item.FisherGroup = double.Parse(ann.Annotations["fisher_group"] as string);
        item.FisherNormal = ann.Annotations["fisher_normal"] as string;
        item.BrglmConverged = ann.Annotations["brglm_converged"] as string;
        item.BrglmGroup = double.Parse(ann.Annotations["brglm_group"] as string);
        item.BrglmScore = ann.Annotations["brglm_score"] as string;
        item.BrglmStrand = ann.Annotations["brglm_strand"] as string;
        item.BrglmPosition = ann.Annotations["brglm_position"] as string;
        item.BrglmGroupFdr = double.Parse(ann.Annotations["brglm_group_fdr"] as string);
        item.Filter = ann.Annotations["filter"] as string;
        item.Identity = ann.Annotations["Identity"] as string;
        result.Add(item);
      }
      return result;
    }

    public void WriteToFile(string fileName, List<FilterItem> t)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("chr\tstart\tend\tmajor_allele\tminor_allele\tref_allele\tnormal_major_count\tnormal_minor_count\ttumor_major_count\ttumor_minor_count\tfisher_group\tfisher_normal\tbrglm_converged\tbrglm_group\tbrglm_score\tbrglm_strand\tbrglm_position\tbrglm_group_fdr\tfilter\tIdentity");
        foreach (var item in t)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}",
            item.Chr,
            item.Start,
            item.End,
            item.MajorAllele,
            item.MinorAllele,
            item.ReferenceAllele,
            item.NormalMajorCount,
            item.NormalMinorCount,
            item.TumorMajorCount,
            item.TumorMinorCount,
            item.FisherGroup,
            item.FisherNormal,
            item.BrglmConverged,
            item.BrglmGroup,
            item.BrglmScore,
            item.BrglmStrand,
            item.BrglmPosition,
            item.BrglmGroupFdr,
            item.Filter,
            item.Identity);
        }
      }
    }
  }
}
