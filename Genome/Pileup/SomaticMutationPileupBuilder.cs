using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using Bio.IO.SAM;
using RCPA.Seq;

namespace CQS.Genome.Pileup
{
  public class SomaticMutationPileupBuilder
  {
    public List<string> Chromosomes { get; private set; }

    private SomaticMutationPileupBuilderOptions options;

    private AlignedPositionMapBuilder normal;

    private AlignedPositionMapBuilder tumor;


    public SomaticMutationPileupBuilder(SomaticMutationPileupBuilderOptions options)
    {
      this.options = options;
    }

    public void Open(string normalFile, string tumorFile)
    {
      GetChromosomes(normalFile);

      normal = new AlignedPositionMapBuilder(options, normalFile);
      tumor = new AlignedPositionMapBuilder(options, tumorFile);
    }

    private void GetChromosomes(string normalFile)
    {
      using (var file = SAMFactory.GetReader(normalFile, false))
      {
        var headers = file.ReadHeaders();
        this.Chromosomes = (from h in headers
                            where h.StartsWith("@SQ")
                            select h.StringAfter("SN:").StringBefore("\t")).ToList();
      }
    }

    public AlignedPositionMapSamples Next()
    {
      while (true)
      {
        var normalItem = normal.Next();
        if (normalItem == null)
        {
          return null;
        }

        var tumorItem = tumor.Next();
        if (tumorItem == null)
        {
          return null;
        }

        while (true)
        {
          while (normalItem.Position < tumorItem.Position)
          {
            normalItem = normal.Next();
            if (normalItem == null)
            {
              return null;
            }
          }

          while (tumorItem.Position < normalItem.Position)
          {
            tumorItem = tumor.Next();
            if (tumorItem == null)
            {
              return null;
            }
          }
        }
      }
    }
  }
}
