using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public class BamSummaryBuilder : AbstractThreadProcessor
  {
    private BamSummaryBuilderOptions _options;
    public BamSummaryBuilder(BamSummaryBuilderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      string rootdir = new DirectoryInfo(_options.InputDir).FullName;

      var statFiles = (from dir in FileUtils.GetRecursiveDirectories(rootdir)
                       let files = Directory.GetFiles(dir, "*.stat")
                       where files.Length > 0
                       from file in files
                       let refile = file.Substring(rootdir.Length+1)
                       select new
                       {
                         File = file,
                         Name = refile
                       }).ToList();

      using (StreamWriter sw = new StreamWriter(_options.OutputFile))
      {
        sw.WriteLine("Path\tName\tTotal\tMapped\tMappedRate\tLReads\tRReads\tPairs\tAlignedPairs\tAlignedPairDiscordant");
        foreach (var file in statFiles)
        {
          var lines = File.ReadAllLines(file.File);
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}%\t{5}\t{6}\t{7}\t{8}\t{9}",
            Path.GetDirectoryName(file.Name),
            Path.GetFileName(file.Name),
            lines[0].StringBefore("+").Trim(),
            lines[2].StringBefore("+").Trim(),
            lines[2].StringAfter("(").StringBefore("%").Trim(),
            lines[4].StringBefore("+").Trim(),
            lines[5].StringBefore("+").Trim(),
            lines[3].StringBefore("+").Trim(),
            lines[6].StringAfter("(").StringBefore("%").Trim(),
            lines[9].StringBefore("+").Trim());
        }
      }

      return new[] { _options.OutputFile };
    }
  }
}
