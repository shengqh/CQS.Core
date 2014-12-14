using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Mirna
{
  public class MirnaCountProcessorCommand : ICommandLineCommand, IToolCommand
  {
    #region IToolCommand Members

    public string GetClassification()
    {
      return "miRNA";
    }

    public string GetCaption()
    {
      return MirnaCountProcessorUI.title;
    }

    public string GetVersion()
    {
      return MirnaCountProcessorUI.version;
    }

    public void Run()
    {
      new MirnaCountProcessorUI().MyShow();
    }

    #endregion

    public string Name
    {
      get { return "mirna_count"; }
    }

    public string Description
    {
      get { return "Parsing miRNA mapping count from bam/sam file"; }
    }

    public bool Process(string[] args)
    {
      MirnaCountProcessorOptions options;
      bool result = true;
      try
      {
        options = CommandLine.Parser.Default.ParseArguments<MirnaCountProcessorOptions>(args,
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
            var files = new MirnaCountProcessor(options).Process();
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
