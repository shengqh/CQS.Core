using System.Diagnostics;

namespace CQS.Genome.Pileup
{
  public class PileupFile : LineFile
  {
    public Process Samtools { get; set; }

    private IPileupItemParser parser;

    public PileupFile(IPileupItemParser parser)
    {
      this.parser = parser;
      this.Samtools = null;
    }

    public PileupItem Next(string chr, long position)
    {
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        if (line.StartsWith("#"))
        {
          continue;
        }

        PileupItem result = parser.GetSequenceIdentifierAndPosition(line);
        if (!result.SequenceIdentifier.Equals(chr) || result.Position != position)
        {
          continue;
        }

        result = parser.GetValue(line);
        return result;
      }

      return null;
    }

    public PileupItem Next()
    {
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        if (line.StartsWith("#"))
        {
          continue;
        }

        PileupItem result = parser.GetValue(line);
        if (result == null)
        {
          continue;
        }

        return result;
      }

      return null;
    }
  }
}
