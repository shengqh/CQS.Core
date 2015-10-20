using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS
{
  public class FileDefinitionBuilderCommand : AbstractCommandLineCommand<FileDefinitionBuilderOptions>
  {
    public override string Name
    {
      get { return "file_def"; }
    }

    public override string Description
    {
      get { return "Build file definition"; }
    }

    public override RCPA.IProcessor GetProcessor(FileDefinitionBuilderOptions options)
    {
      return new FileDefinitionBuilder(options);
    }
  }
}
