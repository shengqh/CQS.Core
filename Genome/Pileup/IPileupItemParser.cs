using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Genome.Pileup
{
  public interface IPileupItemParser : IParser<string, PileupItem>
  {
    PileupItem GetSequenceIdentifierAndPosition(string line);
  }
}
