using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.QC
{
  public class QCSummaryBuilder : AbstractThreadFileProcessor
  {
    private string fastqcDir;

    private string rnaseqcMatrixFile;

    public QCSummaryBuilder(string fastqcDir, string rnaseqcMatrixFile)
    {
      this.fastqcDir = fastqcDir;
      this.rnaseqcMatrixFile = rnaseqcMatrixFile;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      this.Progress.SetMessage("Parsing fastqc result ...");
      var fMap = new FastQCItemReader().ReadFromRootDirectory(fastqcDir).ToDictionary(m => m.Name);

      this.Progress.SetMessage("Parsing RNASeQC result ...");
      var rMap = new RNASeQCItemReader().ReadFromFile(this.rnaseqcMatrixFile).ToDictionary(m => m.Sample);

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Sample\tFileNames\tTotalSequences\tSequenceLength\tGC\t" +
          "AlternativeAlignments\tMappedUnique\tMappedUniqueRate\tMappedPairs\tBaseMismatchRate\t" +
          "IntragenicRate\tExonicRate\tIntronicRate\tIntergenicRate\tExpressionProfilingEfficiency\tTranscriptsDetected\tGenesDetected\tMeanPerBaseCoverage\tFragmentLengthMean\tFragmentLengthStdDev");
        var keys = (from k in fMap.Keys
                    orderby k
                    select k).ToList();
        foreach (var key in keys)
        {
          var fitem = fMap[key];
          var ritem = rMap[key];
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7:0.000}\t{8}\t{9:0.000}\t{10:0.000}\t{11:0.000}\t{12:0.000}\t{13:0.000}\t{14:0.000}\t{15}\t{16}\t{17}\t{18}\t{19}",
            fitem.Name,
            fitem.FileNames,
            fitem.TotalSequences,
            fitem.SequenceLength,
            fitem.GC,
            ritem.AlternativeAlignments,
            ritem.MappedUnique,
            ritem.MappedUnique * 1.0 / fitem.TotalSequences,
            ritem.MappedPairs,
            ritem.BaseMismatchRate,
            ritem.IntragenicRate,
            ritem.ExonicRate,
            ritem.IntronicRate,
            ritem.IntergenicRate,
            ritem.ExpressionProfilingEfficiency,
            ritem.TranscriptsDetected,
            ritem.GenesDetected,
            ritem.MeanPerBaseCoverage,
            ritem.FragmentLengthMean,
            ritem.FragmentLengthStdDev
            );
        }
      }

      return new string[] { fileName };
    }
  }
}
