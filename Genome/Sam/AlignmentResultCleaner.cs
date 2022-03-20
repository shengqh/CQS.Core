using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Sam
{
  /// <summary>
  ///   Remove template sam/bam file if the sorted version bam file exists
  /// </summary>
  public class AlignmentResultCleaner : AbstractThreadProcessor
  {
    private readonly AlignmentResultCleanerOptions _options;

    public AlignmentResultCleaner(AlignmentResultCleanerOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var dirs = new Queue<string>();
      dirs.Enqueue(_options.RootDirectory);

      Console.WriteLine("Processing at {0} ...", _options.DeletionMode ? "deletion mode" : "view mode");
      var prefix = _options.DeletionMode ? "Deleting " : "Can delete ";
      while (dirs.Count > 0)
      {
        var dir = dirs.Dequeue();
        var bamfile = Directory.GetFiles(dir, "*.bam");
        if (bamfile.Length > 0)
        {
          var waitingList = new List<string>();

          waitingList.AddRange(Directory.GetFiles(dir, "*.sai"));

          var hasSortedBam = bamfile.Any(m => m.ToLower().EndsWith("sorted.bam"));
          foreach (var bam in bamfile)
          {
            if (hasSortedBam && !bam.ToLower().EndsWith("sorted.bam"))
            {
              waitingList.Add(bam);
            }
            var sam = Path.ChangeExtension(bam, ".sam");
            if (File.Exists(sam))
            {
              waitingList.Add(sam);
            }
            var baiFile = Path.ChangeExtension(bam, ".bai");
            if (File.Exists(baiFile))
            {
              waitingList.Add(baiFile);
            }
          }

          foreach (var file in waitingList)
          {
            Progress.SetMessage(prefix + file);

            if (_options.DeletionMode)
            {
              File.Delete(file);
            }
          }
        }

        var subdirs = Directory.GetDirectories(dir);
        foreach (var subdir in subdirs)
        {
          dirs.Enqueue(subdir);
        }
      }

      return new string[] { };
    }
  }
}