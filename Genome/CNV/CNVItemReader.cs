using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.CNV
{
  public class CNVItemReader<T> : AbstractHeaderFile<T> where T : CNVItem, new()
  {
    public CNVItemReader() : base() { }

    public CNVItemReader(string filename) : base(filename) { }

    protected override Dictionary<string, Action<string, T>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, T>>();

      result["sample"] = CNVItemUtils.FuncFileName;
      result["sampleID"] = CNVItemUtils.FuncFileName;
      result["sampleName"] = CNVItemUtils.FuncFileName;
      result["chr"] = CNVItemUtils.FuncChrom;
      result["chromosome"] = CNVItemUtils.FuncChrom;
      result["seqnames"] = CNVItemUtils.FuncChrom;
      result["start"] = CNVItemUtils.FuncChromStart;
      result["end"] = CNVItemUtils.FuncChromEnd;
      result["stop"] = CNVItemUtils.FuncChromEnd;
      result["type"] = CNVItemUtils.FuncItemType;
      result["state"] = CNVItemUtils.FuncItemType;

      return result;
    }
  }
}
