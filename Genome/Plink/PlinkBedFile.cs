using RCPA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkBedFile
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
      result.Locus = PlinkLocus.ReadFromFile(bimFile);
      result.AllocateDataMemory();

      OpenBinaryFile(fileName);
      try
      {
        if (IsSNPMajor)
        {
          for (int i = 0; i < result.Locus.Count; i++)
          {
            int j = 0;
            Console.WriteLine("Locus {0} : {1}", i, _reader.BaseStream.Position);
            while (j < result.Individual.Count)
            {
              var b = ReadByte();
              int c = 0;
              while (c < 7 && j < result.Individual.Count)
              {
                result.One[i, j] = b[c++];
                result.Two[i, j] = b[c++];
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
                result.One[j, i] = b[c++];
                result.Two[j, i] = b[c++];
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

    public void OpenBinaryFile(string fileName)
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
            Console.WriteLine("Detected that binary PED file is v1.00 SNP-major mode\n");
          else
            Console.WriteLine("Detected that binary PED file is v1.00 individual-major mode\n");

        }
        else v1_bfile = false;

      }
      else v1_bfile = false;


      // Reset file if < v1
      if (!v1_bfile)
      {
        Console.WriteLine("Warning, old BED file <v1.00 : will try to recover...\n");
        DoOpenFile(fileName);
        b = ReadByte();
      }

      // If 0.99 file format
      if ((!v1_bfile) && (b[1] || b[2] || b[3] || b[4] || b[5] || b[6] || b[7]))
      {
        Console.WriteLine(" *** Possible problem: guessing that BED is < v0.99   *** ");
        Console.WriteLine(" *** High chance of data corruption, spurious results *** ");

        IsSNPMajor = false;
        DoOpenFile(fileName);
      }
      else if (!v1_bfile)
      {
        IsSNPMajor = b[0];

        Console.WriteLine("Binary PED file is v0.99\n");

        if (IsSNPMajor)
          Console.WriteLine("Detected that binary PED file is in SNP-major mode\n");
        else
          Console.WriteLine("Detected that binary PED file is in individual-major mode\n");
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
  }
}
