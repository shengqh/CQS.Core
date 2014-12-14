using CQS.Genome.Sam;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class MappedReadBuilder : AbstractThreadProcessor
  {
    private MappedReadBuilderOptions _options;

    public MappedReadBuilder(MappedReadBuilderOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var counts = ReadCountItem.ReadFromFile(_options.InputFile);
      Progress.SetMessage("There are {0} reads in count file", counts.Count);

      if (_options.MinQueryCount > 1)
      {
        counts.RemoveAll(m => m.Count < _options.MinQueryCount);
        Progress.SetMessage("There are {0} reads in count file with query count larger than/equal to {1}", counts.Count, _options.MinQueryCount);
      }

      if (_options.Unmapped)
      {
        foreach (var readNameFile in _options.MappedFiles)
        {
          HashSet<string> reads = ReadPerfectMappedReadNames(readNameFile);

          counts.RemoveAll(m => reads.Contains(m.Name));

          Progress.SetMessage("After removing perfect mapped reads, {0} reads left.", counts.Count);
        }
      }
      else
      {
        for (int i = 0; i < _options.MappedFiles.Count; i++)
        {
          var readNameFile = _options.MappedFiles[i];
          var name = _options.FileTags[i];
          HashSet<string> reads = ReadPerfectMappedReadNames(readNameFile);
          counts.ForEach(m =>
          {
            if (reads.Contains(m.Name))
            {
              if (string.IsNullOrEmpty(m.MappedFile))
              {
                m.MappedFile = name;
              }
              else
              {
                m.MappedFile = m.MappedFile + " | " + name;
              }
            }
          });
        }

        counts.RemoveAll(m => string.IsNullOrEmpty(m.MappedFile));

        Progress.SetMessage("After removing non-perfect mapped reads, {0} reads left.", counts.Count);
      }

      ReadCountItem.WriteToFile(_options.OutputFile, counts, true);

      return new[] { _options.OutputFile };
    }

    private HashSet<string> ReadPerfectMappedReadNames(string readNameFile)
    {
      HashSet<string> reads = new HashSet<string>();

      var ext = Path.GetExtension(readNameFile).ToLower();
      if (!ext.Equals(".bam") && !ext.Equals(".sam"))
      {
        Progress.SetMessage("Reading perfect mapped reads from text file {0} ...", readNameFile);
        reads = new HashSet<string>(File.ReadAllLines(readNameFile));
      }
      else
      {
        Progress.SetMessage("Reading perfect mapped reads from bam/sam file {0} ...", readNameFile);
        var list = new List<string>();
        using (var sr = SAMFactory.GetReader(readNameFile, _options.Samtools, true))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (line.Contains("NM:i:0"))
            {
              list.Add(line.StringBefore("\t"));
            }
          }
        }
        reads = new HashSet<string>(list);
      }

      Progress.SetMessage("{0} perfect mapped reads.", reads.Count);
      return reads;
    }
  }
}
