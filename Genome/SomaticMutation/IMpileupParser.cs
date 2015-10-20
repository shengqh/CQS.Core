using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SomaticMutation
{
  public interface IMpileupParser
  {
    MpileupFisherResult Parse(string line, bool writeCandidateFile);
  }
}
