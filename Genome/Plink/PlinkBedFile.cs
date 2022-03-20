using RCPA;
using RCPA.Gui;
using System.Collections;
using System.IO;

namespace CQS.Genome.Plink
{
  public class PlinkBedFile : ProgressClass
  {
    private BinaryReader _reader;
    private long _startPosition;

    public bool IsSNPMajor { get; set; }

    public PlinkBedFile()
    { }

    public PlinkData ReadFromFile(string fileName)
    {
      var famFile = FileUtils.ChangeExtension(fileName, ".fam");
      if (!File.Exists(famFile))
      {
        throw new FileNotFoundException("File not found: " + famFile);
      }

      var bimFile = FileUtils.ChangeExtension(fileName, ".bim");
      if (!File.Exists(bimFile))
      {
        throw new FileNotFoundException("File not found: " + bimFile);
      }

      var result = new PlinkData();
      result.Individual = PlinkIndividual.ReadFromFile(famFile);
      result.Locus = PlinkLocus.ReadFromBimFile(bimFile);
      result.AllocateDataMemory();

      OpenBinaryFile(fileName);
      try
      {
        if (IsSNPMajor)
        {
          for (int i = 0; i < result.Locus.Count; i++)
          {
            int j = 0;
            while (j < result.Individual.Count)
            {
              var b = ReadByte();
              int c = 0;
              while (c < 7 && j < result.Individual.Count)
              {
                result.IsHaplotype1Allele2[i, j] = b[c++];
                result.IsHaplotype2Allele2[i, j] = b[c++];
                j++;
              }
            }
          }
        }
        else
        {
          for (int i = 0; i < result.Individual.Count; i++)
          {
            int j = 0;
            while (j < result.Locus.Count)
            {
              var b = ReadByte();
              int c = 0;
              while (c < 7 && j < result.Locus.Count)
              {
                result.IsHaplotype1Allele2[j, i] = b[c++];
                result.IsHaplotype2Allele2[j, i] = b[c++];
                j++;
              }
            }
          }
        }
      }
      finally
      {
        _reader.Close();
        _reader = null;
      }

      return result;
    }

    protected void OpenBinaryFile(string fileName)
    {
      DoOpenFile(fileName);

      BitArray b = ReadByte();

      bool v1_bfile = true;

      if ((b[2] && b[3] && b[5] && b[6]) && !(b[0] || b[1] || b[4] || b[7]))
      {

        // Next number
        b = ReadByte();
        if ((b[0] && b[1] && b[3] && b[4]) && !(b[2] || b[5] || b[6] || b[7]))
        {
          b = ReadByte();
          IsSNPMajor = b[0];

          if (IsSNPMajor)
            Progress.SetMessage("Detected that {0} is v1.00 SNP-major mode", fileName);
          else
            Progress.SetMessage("Detected that {0} is v1.00 individual-major mode", fileName);

        }
        else v1_bfile = false;

      }
      else v1_bfile = false;


      // Reset file if < v1
      if (!v1_bfile)
      {
        Progress.SetMessage("Warning, old BED file <v1.00 : will try to recover...\n");
        DoOpenFile(fileName);
        b = ReadByte();
      }

      // If 0.99 file format
      if ((!v1_bfile) && (b[1] || b[2] || b[3] || b[4] || b[5] || b[6] || b[7]))
      {
        Progress.SetMessage(" *** Possible problem: guessing that BED is < v0.99   *** ");
        Progress.SetMessage(" *** High chance of data corruption, spurious results *** ");

        IsSNPMajor = false;
        DoOpenFile(fileName);
      }
      else if (!v1_bfile)
      {
        IsSNPMajor = b[0];

        Progress.SetMessage("Binary PED file is v0.99\n");

        if (IsSNPMajor)
          Progress.SetMessage("Detected that {0} is in SNP-major mode", fileName);
        else
          Progress.SetMessage("Detected that {0} is in individual-major mode", fileName);
      }

      _startPosition = _reader.BaseStream.Position;
    }

    private void DoOpenFile(string fileName)
    {
      if (_reader != null)
      {
        _reader.Close();
        _reader = null;
      }
      _reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
    }

    private BitArray ReadByte()
    {
      var magic = _reader.ReadByte();
      return new BitArray(new byte[] { magic });
    }

    public void Close()
    {
      if (_reader != null)
      {
        _reader.Close();
        _reader = null;
      }
    }
  }
}
