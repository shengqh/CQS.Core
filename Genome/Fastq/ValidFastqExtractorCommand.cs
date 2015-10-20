using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Fastq
{
  public class ValidFastqExtractorCommand : AbstractCommandLineCommand<ValidFastqExtractorOptions>
  {
    public override string Name
    {
      get { return "fastq_valid_extractor"; }
    }

    public override string Description
    {
      get { return "Extract valid fastq entries from truncated/damaged fastq file"; }
    }

    public override RCPA.IProcessor GetProcessor(ValidFastqExtractorOptions options)
    {
      return new ValidFastqExtractor(options);
    }
  }
}
