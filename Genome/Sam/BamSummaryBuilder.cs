using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                       let refile = file.Substring(rootdir.Length + 1)
                       select new
                       {
                         File = file,
                         Name = refile
                       }).ToList();

      using (StreamWriter sw = new StreamWriter(_options.OutputFile))
      {
        sw.WriteLine("Path{0}\tTotal\tMapped\tMappedRate\tLReads\tRReads\tPairs\tAlignedPairs\tAlignedPairDiscordant",
          _options.ExcludeFileName ? "" : "\tName");
        foreach (var file in statFiles)
        {
          var lines = File.ReadAllLines(file.File);
          sw.Write("{0}", Path.GetDirectoryName(file.Name));
          if (!_options.ExcludeFileName)
          {
            sw.Write("\t{0}", Path.GetFileName(file.Name));
          }
          sw.WriteLine("\t{0}\t{1}\t{2}%\t{3}\t{4}\t{5}\t{6}%\t{7}",
            FindLine(lines, " in total ").StringBefore("+").Trim(),
            FindLine(lines, " mapped ").StringBefore("+").Trim(),
            FindLine(lines, " mapped ").StringAfter("(").StringBefore("%").Trim(),
            FindLine(lines, " read1").StringBefore("+").Trim(),
            FindLine(lines, " read2").StringBefore("+").Trim(),
            FindLine(lines, " paired in sequencing").StringBefore("+").Trim(),
            FindLine(lines, " properly paired").StringAfter("(").StringBefore("%").Trim(),
            FindLine(lines, " mapped to a different chr").StringBefore("+").Trim());
        }
      }

      return new[] { _options.OutputFile };
    }

    private static string FindLine(string[] lines, string key)
    {
      return lines.Where(l => l.Contains(key)).First();
    }
  }
}
