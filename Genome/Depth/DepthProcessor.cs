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

namespace CQS.Genome.Depth
{

  public class DepthProcessor : AbstractThreadProcessor
  {
    private DepthProcessorOptions _options;

    public DepthProcessor(DepthProcessorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      TextReader reader;
      if (string.IsNullOrEmpty(_options.InputFile))
      {
        reader = Console.In;
      }
      else
      {
        reader = new StreamReader(_options.InputFile);
      }

      TextWriter writer;
      if (string.IsNullOrEmpty(_options.OutputFile))
      {
        writer = Console.Out;
      }
      else
      {
        writer = new StreamWriter(_options.OutputFile);
      }

      try
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          bool bFailed = false;
          for (int i = 2; i < parts.Length; i++)
          {
            if (int.Parse(parts[i]) < _options.MinimimDepthInEachSample)
            {
              bFailed = true;
              break;
            }
          }

          if (!bFailed)
          {
            writer.WriteLine(line);
          }
        }
      }
      finally
      {
        try
        {
          if (reader != Console.In)
          {
            reader.Close();
          }

          if (writer != Console.Out)
          {
            writer.Close();
          }
        }
        catch
        { }
      }

      if (!string.IsNullOrEmpty(_options.OutputFile))
      {
        return new string[] { _options.OutputFile };
      }
      else
      {
        return null;
      }
    }
  }
}
