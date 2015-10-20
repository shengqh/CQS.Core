using RCPA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkBedRandomFile : IDisposable
  {
    private BinaryReader _reader;
    private long _startPosition;

    public PlinkData Data { get; private set; }

    public bool IsSNPMajor { get; private set; }

    public PlinkBedRandomFile()
    { }

    public PlinkBedRandomFile(string fileName)
    {
      OpenBinaryFile(fileName);
    }

    public bool[,] Read(string name)
    {
      bool[,] result;
      if (IsSNPMajor)
      {
        result = new bool[2, Data.Individual.Count];

        int locusIndex;

        if (!this.Data.LocusMap.TryGetValue(name, out locusIndex))
        {
          throw new Exception("Cannot find locus with name " + name);
        }
        long individualSize = Data.Individual.Count % 4 == 0 ? Data.Individual.Count / 4 : ((int)(Data.Individual.Count / 4) + 1);
        _reader.BaseStream.Position = _startPosition + locusIndex * individualSize;

        int j = 0;
        while (j < Data.Individual.Count)
        {
          var b = ReadByte();
          int c = 0;
          while (c < 7 && j < Data.Individual.Count)
          {
            result[0, j] = b[c++];
            result[1, j] = b[c++];
            j++;
          }
        }
      }
      else
      {
        result = new bool[2, Data.Locus.Count];

        var individualIndex = this.Data.IndividualMap[name];
        var passedIndividual = (int)Math.Floor(Data.Locus.Count / 4.0);
        _reader.BaseStream.Position = _startPosition + passedIndividual * sizeof(char);

        int j = 0;
        while (j < Data.Locus.Count)
        {
          var b = ReadByte();
          int c = 0;
          while (c < 7 && j < Data.Locus.Count)
          {
            result[0, j] = b[c++];
            result[1, j] = b[c++];
            j++;
          }
        }
      }

      return result;
    }

    public void OpenBinaryFile(string fileName)
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

      Data = new PlinkData();
      Data.Individual = PlinkIndividual.ReadFromFile(famFile);
      Data.Locus = PlinkLocus.ReadFromBimFile(bimFile);
      //Data.Locus.ForEach(m => m.MarkerId = m.MarkerId.ToLower());
      Data.BuildMap();

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
      Close();

      _reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
    }

    private BitArray ReadByte()
    {
      var magic = _reader.ReadByte();
      return new BitArray(new byte[] { magic });
    }

    public void Close()
    {
      if (null != _reader)
      {
        _reader.Close();
        _reader = null;
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!m_disposed)
      {
        if (disposing)
        {
          // Release managed resources
        }

        if (null != _reader)
        {
          _reader.Close();
          _reader = null;
        }

        m_disposed = true;
      }
    }

    ~PlinkBedRandomFile()
    {
      Dispose(false);
    }

    private bool m_disposed;

    #endregion

  }
}
