using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.QC
{
  public class RNASeQCItemReader : AbstractHeaderFile<RNASeQCItem>
  {
    protected override Dictionary<string, Action<string, RNASeQCItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, RNASeQCItem>>();

      result["Sample"] = (m, n) => n.Sample = m;
      result["Alternative Aligments"] = (m, n) => n.AlternativeAlignments = long.Parse(m);
      result["Mapped Unique"] = (m, n) => n.MappedUnique = long.Parse(m);
      result["Base Mismatch Rate"] = (m, n) => n.BaseMismatchRate = double.Parse(m);
      result["Mapped Pairs"] = (m, n) => n.MappedPairs = long.Parse(m);
      result["Fragment Length Mean"] = (m, n) => n.FragmentLengthMean = int.Parse(m);
      result["Fragment Length StdDev"] = (m, n) => n.FragmentLengthStdDev = int.Parse(m);
      result["Chimeric Pairs"] = (m, n) => n.ChimericPairs = long.Parse(m);
      result["Intragenic Rate"] = (m, n) => n.IntragenicRate = double.Parse(m);
      result["Intronic Rate"] = (m, n) => n.IntronicRate = double.Parse(m);
      result["Exonic Rate"] = (m, n) => n.ExonicRate = double.Parse(m);
      result["Read Length"] = (m, n) => n.ReadLength = int.Parse(m);
      result["Transcripts Detected"] = (m, n) => n.TranscriptsDetected = int.Parse(m);
      result["Genes Detected"] = (m, n) => n.GenesDetected = int.Parse(m);
      result["Mean Per Base Cov."] = (m, n) => n.MeanPerBaseCoverage = double.Parse(m);
      result["Expression Profiling Efficiency"] = (m, n) => n.ExpressionProfilingEfficiency = double.Parse(m);

      return result;
    }
  }
}
