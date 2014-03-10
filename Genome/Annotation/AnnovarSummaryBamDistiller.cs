using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using CQS.Genome.Affymetrix;
using System.Drawing;
using System.Security.Policy;

namespace CQS.Genome.Annotation
{
  public class AnnovarSummaryBamDistiller : AbstractThreadFileProcessor
  {
    private string affyAnnotationFile;
    private string bamFile;
    private string targetDir;
    private string suffix;

    public AnnovarSummaryBamDistiller(string affyAnnotationFile, string bamFile, string targetDir, string suffix)
    {
      this.affyAnnotationFile = Path.GetFullPath(affyAnnotationFile);
      this.bamFile = Path.GetFullPath(bamFile);
      this.targetDir = Path.GetFullPath(targetDir);
      this.suffix = suffix;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      Dictionary<string, string> genenames = File.Exists(this.affyAnnotationFile) ? AnnotationFile.GetGeneSymbolDescriptionMap(this.affyAnnotationFile) : new Dictionary<string, string>();

      var items = new AnnovarGenomeSummaryItemReader().ReadFromFile(fileName);
      var shFile = this.targetDir + "/" + suffix + ".sh";
      using (StreamWriter sw = new StreamWriter(shFile))
      {

        foreach (var item in items)
        {
          var targetFile = string.Format("{0}/{1}_{2}-{3}_{4}_{5}",
              this.targetDir,
              item.Seqname,
              item.Start,
              item.End,
              (from g in item.Genes
               select g.Name).Merge("_"),
              this.suffix);

          sw.WriteLine("echo \"{0}.bam\"", Path.GetFileName(targetFile));
          sw.WriteLine("samtools view -b {0} {1}:{2}-{3} | samtools sort - {4}",
            this.bamFile,
            item.Seqname,
            item.Start,
            item.End,
            targetFile);

          sw.WriteLine("samtools index {0}.bam", targetFile);
        }
      }

      return new string[] { shFile };
    }
  }
}

