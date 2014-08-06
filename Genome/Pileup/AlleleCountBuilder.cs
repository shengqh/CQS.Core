using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Genome.Sam;
using Bio.IO.SAM;
using CQS.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Mirna;
using RCPA.Seq;
using CQS.Genome.Gtf;
using CQS.Genome.Vcf;
using CQS.Genome.SomaticMutation;
using System.Diagnostics;

namespace CQS.Genome.Pileup
{

  public class AlleleCountBuilder : AbstractThreadProcessor
  {
    private AlleleCountBuilderOptions _options;

    public AlleleCountBuilder(AlleleCountBuilderOptions options)
    {
      this._options = options;
    }

    private Process ExecuteSamtools(List<KeyValuePair<string, string>> bamList)
    {
      var mapq = _options.MinimumReadQuality == 0 ? "" : "-q " + _options.MinimumReadQuality.ToString();
      var baseq = _options.MinimumBaseQuality == 0 ? "" : "-Q " + _options.MinimumBaseQuality.ToString();
      var result = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = _options.Samtools,
          Arguments = string.Format(" mpileup -A {0} {1} -f {2} {3} ", mapq, baseq, _options.GenomeFastaFile, bamList.ConvertAll(m => m.Key).Merge(" ")),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        }
      };

      Console.Out.WriteLine("running command : " + result.StartInfo.FileName + " " + result.StartInfo.Arguments);
      try
      {
        if (!result.Start())
        {
          Console.Error.WriteLine(
            "samtools mpileup cannot be started, check your parameters and ensure that samtools are available.");
          return null;
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(
          "samtools mpileup cannot be started, check your parameters and ensure that samtools are available : {0}",
          ex.Message);
        return null;
      }

      return result;
    }

    public override IEnumerable<string> Process()
    {
      var bamList = ReadBamList();

      var process = ExecuteSamtools(bamList);
      if (process == null)
      {
        throw new Exception("Fail to execute samtools.");
      }

      var vcfItems = new VcfItemListFormat().ReadFromFile(_options.InputFile);
      vcfItems.Header = vcfItems.Header + "\t" + bamList.ConvertAll(m => m.Value).Merge("\t");

      var vcfMap = vcfItems.Items.ToDictionary(m => GetKey(m.Seqname, m.Start));

      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      pfile.Open(process.StandardOutput);

      try
      {
        using (pfile)
        {
          string line;
          while ((line = pfile.ReadLine()) != null)
          {
            var item = parser.GetSequenceIdentifierAndPosition(line);
            var key = GetKey(item.SequenceIdentifier, item.Position);

            VcfItem vcf;
            if (!vcfMap.TryGetValue(key, out vcf))
            {
              continue;
            }

            item = parser.GetValue(line);
            foreach (var sample in item.Samples)
            {
              var refCount = sample.Count(m => m.Event.Equals(vcf.RefAllele));
              var altCount = sample.Count(m => m.Event.Equals(vcf.AltAllele));
              vcf.Line = vcf.Line + string.Format("\t{0}:{1}", refCount, altCount);
            }
          }
        }
      }
      finally
      {
        try
        {
          if (process != null) process.Kill();
        }
        catch
        { }
      }

      new VcfItemListFormat().WriteToFile(_options.OutputFile, vcfItems);

      return new string[] { _options.OutputFile };
    }

    private List<KeyValuePair<string, string>> ReadBamList()
    {
      return (from line in File.ReadAllLines(_options.ListFile)
              let parts = line.Split('\t')
              select new KeyValuePair<string, string>(parts[0], parts[1])).ToList();
    }

    private string GetKey(string seqname, long position)
    {
      return string.Format("{0}_{1}", seqname.StringAfter("chr"), position);
    }
  }
}
