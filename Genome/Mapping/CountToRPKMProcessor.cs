using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.Mapping
{
  public class CountToRPKMProcessor:AbstractThreadProcessor
  {
    private CountToRPKMProcessorOptions options;

    public CountToRPKMProcessor(CountToRPKMProcessorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {

      throw new NotImplementedException();
    }
  }
}
