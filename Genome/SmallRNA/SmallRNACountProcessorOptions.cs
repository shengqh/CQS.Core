using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using RCPA.Seq;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using CQS.Genome.Gsnap;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountProcessorOptions : AbstractSmallRNACountProcessorOptions, ICountProcessorOptions
  {
    public SmallRNACountProcessorOptions()
    {
    }
  }
}
