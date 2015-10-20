using RCPA;
using RCPA.Gui;
using RCPA.R;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public class FilterProcessor : AbstractThreadProcessor
  {
    private readonly FilterProcessorOptions _options;

    public FilterProcessor(FilterProcessorOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("filter process started at {0}", DateTime.Now);
      var watch = new Stopwatch();
      watch.Start();

      var tsvfile = _options.OutputFile + ".rtsv";

      var roptions = new RProcessorOptions()
      {
        RExecute = _options.GetRCommand(),
        RFile = _options.TargetRFile,
        ExpectResultFile = _options.ROutputFile
      };

      new RProcessor(roptions).Process();

      if (!File.Exists(_options.ROutputFile))
      {
        throw new Exception(string.Format("R command failed, look at the file {0}!\nMake sure that your R and R packages brglm, stringr have been installed.", roptions.RFile + ".log"));
      }
      else if (!_options.IsValidation)
      {
        var items = new FilterItemTextFormat().ReadFromFile(_options.ROutputFile);

        var unfilteredfile = Path.ChangeExtension(_options.ROutputFile, ".vcf");
        new FilterItemVcfWriter(_options).WriteToFile(unfilteredfile, items);

        items.RemoveAll(m => !m.Filter.Equals("PASS"));
        var vcfFile = Path.ChangeExtension(_options.OutputFile, ".vcf");
        new FilterItemVcfWriter(_options).WriteToFile(vcfFile, items);

        new FilterItemTextFormat().WriteToFile(_options.OutputFile, items);
      }

      watch.Stop();
      Progress.SetMessage("filter process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      return new[] { _options.OutputFile };
    }
  }
}