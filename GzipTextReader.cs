﻿namespace CQS
{
  public class GzipTextReader : AbstractProcessReader
  {
    public GzipTextReader(string gzip) : base(gzip) { }

    public GzipTextReader(string gzip, string filename) : base(gzip, filename) { }

    protected override string GetProcessArguments(string filename)
    {
      return string.Format("-cd {0}", filename);
    }

    public override bool NeedProcess(string filename)
    {
      return filename.ToLower().EndsWith(".gz");
    }
  }
}
