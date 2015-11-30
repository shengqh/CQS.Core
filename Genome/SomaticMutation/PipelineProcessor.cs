using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CQS.Genome.Annotation;
using RCPA.Gui;
using RCPA;

namespace CQS.Genome.SomaticMutation
{
  public class PipelineProcessor : AbstractThreadProcessor
  {
    private readonly PipelineProcessorOptions _options;

    public PipelineProcessor(PipelineProcessorOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var filterOptions = _options.GetFilterOptions();
      if (!File.Exists(filterOptions.InputFile))
      {
        //run initialize candidates
        _options.GetProcessor().Process();
      }

      //check the result exists
      filterOptions.IsPileup = false;
      if (!filterOptions.PrepareOptions())
      {
        throw new Exception("Filter options failed: " + filterOptions.ParsingErrors.Merge("\n"));
      }

      var annotationOptions = _options.GetAnnotationOptions();
      if (!File.Exists(annotationOptions.InputFile))
      {
        new FilterProcessor(filterOptions).Process();
      }

      annotationOptions.IsPileup = false;
      if (!annotationOptions.PrepareOptions())
      {
        throw new Exception("Annotation options failed : " + annotationOptions.ParsingErrors.Merge("\n"));
      }

      new AnnotationProcessor(annotationOptions).Process();
      return new[] { annotationOptions.AnnovarOutputFile };
    }
  }
}