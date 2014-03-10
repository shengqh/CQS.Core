using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.CNV
{
  public class CNVItemReader : AbstractHeaderFile<CNVItem>
  {
    public CNVItemReader() : base() { }

    public CNVItemReader(string filename) : base(filename) { }

    protected override Dictionary<string, Action<string, CNVItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, CNVItem>>();

      result["sample"] = CNVItemUtils.FuncFileName;
      result["sampleID"] = CNVItemUtils.FuncFileName;
      result["chr"] = CNVItemUtils.FuncChrom;
      result["chromosome"] = CNVItemUtils.FuncChrom;
      result["start"] = CNVItemUtils.FuncChromStart;
      result["end"] = CNVItemUtils.FuncChromEnd;
      result["stop"] = CNVItemUtils.FuncChromEnd;
      result["type"] = CNVItemUtils.FuncItemType;
      result["state"] = CNVItemUtils.FuncItemType;

      return result;
    }
  }
}
