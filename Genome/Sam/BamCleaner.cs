using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Sam
{
  public class BamCleaner : AbstractThreadProcessor
  {
    private BamCleanerOptions _options;
    public BamCleaner(BamCleanerOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      string rootdir = new DirectoryInfo(_options.InputDir).FullName;

      foreach (var dir in FileUtils.GetRecursiveDirectories(rootdir))
      {
        var bamfiles = Directory.GetFiles(dir, "*.bam").OrderByDescending(m => m.Length).ToList();
        if (bamfiles.Count > 0)
        {
          for (int i = 1; i < bamfiles.Count; i++)
          {
            File.Delete(bamfiles[i]);
            if (File.Exists(bamfiles[i] + ".bai"))
            {
              File.Delete(bamfiles[i] + ".bai");
            }
            if (File.Exists(Path.ChangeExtension(bamfiles[i], ".bai")))
            {
              File.Delete(Path.ChangeExtension(bamfiles[i], ".bai"));
            }
          }
          Progress.SetMessage("{0} -> {1}", dir, Path.GetFileName(bamfiles[0]));
        }
      }
      return null;
    }
  }
}
