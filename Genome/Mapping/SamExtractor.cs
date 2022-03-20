using CQS.Genome.Sam;
using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class SamExtractor : AbstractThreadProcessor
  {
    private readonly SamExtractorOptions _options;

    public SamExtractor(SamExtractorOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var format = new MappedItemGroupXmlFileFormat();

      Progress.SetMessage("reading mapped reads from " + _options.CountFile + " ...");
      var mapped = format.ReadFromFile(_options.CountFile);

      var sequenceLocusSet = new HashSet<string>(from item in mapped
                                                 from mi in item
                                                 from mr in mi.MappedRegions
                                                 from al in mr.AlignedLocations
                                                 select string.Format("{0}:{1}:{2}", al.Parent.Sequence, al.Seqname, al.Start));
      Progress.SetMessage("There are {0} unique sequence:locus", sequenceLocusSet.Count);

      using (var sw = new StreamWriter(_options.OutputFile))
      {
        using (var sr = SAMFactory.GetReader(_options.BamFile, false))
        {
          sr.ReadHeaders().ForEach(m => sw.WriteLine(m));

          int count = 0;
          int accepted = 0;
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (count % 1000 == 0)
            {
              if (Progress.IsCancellationPending())
              {
                throw new UserTerminatedException();
              }
            }

            if (count % 100000 == 0 && count > 0)
            {
              Progress.SetMessage("{0} candidates from {1} reads", accepted, count);
            }

            count++;

            var parts = line.Split('\t');

            var locus = string.Format("{0}:{1}:{2}", parts[SAMFormatConst.SEQ_INDEX], parts[SAMFormatConst.RNAME_INDEX], parts[SAMFormatConst.POS_INDEX]);
            if (!sequenceLocusSet.Contains(locus))
            {
              continue;
            }

            sw.WriteLine(line);
            accepted++;
          }
        }
      }

      return new[] { _options.OutputFile };
    }
  }
}