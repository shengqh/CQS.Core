using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessorCommand : ICommandLineCommand, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "Mapping";
    }

    public string GetCaption()
    {
      return MappedCountProcessorUI.title;
    }

    public string GetVersion()
    {
      return MappedCountProcessorUI.version;
    }

    public void Run()
    {
      new MappedCountProcessorUI().MyShow();
    }

    #endregion

    public string Name
    {
      get { return "mapped_count"; }
    }

    public string Description
    {
      get { return "Parsing mapping count from bam/sam file"; }
    }

    public bool Process(string[] args)
    {
     MappedCountProcessorOptions options;
      bool result = true;
      try
      {
        options = CommandLine.Parser.Default.ParseArguments<MappedCountProcessorOptions>(args,
          () =>
          {
            result = false;
          }
        );

        if (result)
        {
          if (!options.PrepareOptions())
          {
            Console.Out.WriteLine(options.GetUsage());
            result = false;
          }
          else
          {
            var files = new MappedCountProcessor(options).Process();
            Console.WriteLine("File saved to :\n" + files.Merge("\n"));
          }
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
        result = false;
      }

      return result;
    }
  }
}
