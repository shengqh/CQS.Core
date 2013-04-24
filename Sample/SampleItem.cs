﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Sample
{
  public class SampleItem : Annotation
  {
    public SampleItem()
    {
      Dataset = StatusValue.NA;
      Sample = StatusValue.NA;
      SourceName = StatusValue.NA;
      SampleTitle = StatusValue.NA;
    }

    public string Dataset { get; set; }
    public string Sample { get; set; }
    public string SourceName { get; set; }
    public string SampleTitle { get; set; }
  }
}