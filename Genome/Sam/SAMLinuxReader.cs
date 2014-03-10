using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Bio.IO.BAM;
using Bio.IO.SAM;

namespace CQS.Genome.Sam
{
  public class SAMLinuxReader : AbstractProcessReader, ISAMFile
  {
    public SAMLinuxReader(string samtools)
      : base(samtools)
    { }

    public SAMLinuxReader(string samtools, string filename)
      : this(samtools)
    {
      OpenWithException(filename);
    }

    public override bool NeedProcess(string filename)
    {
      return SAMUtils.IsBAMFile(filename);
    }

    protected override string GetProcessArguments(string filename)
    {
      return string.Format("view -h {0}", filename);
    }

    public List<string> ReadHeaders()
    {
      List<string> result = new List<string>();
      while (this.reader.Peek() == (int)'@')
      {
        result.Add(this.reader.ReadLine());
      }
      return result;
    }
  }
}
