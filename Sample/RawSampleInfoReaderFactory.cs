using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Ncbi.Geo;
using System.IO;
using RCPA;

namespace CQS.Sample
{
  public static class RawSampleInfoReaderFactory
  {
    private static List<IRawSampleInfoReader> readers;

    public static List<IRawSampleInfoReader> Readers
    {
      get
      {
        if (readers == null)
        {
          readers = new List<IRawSampleInfoReader>();
          readers.Add(new SdrfSampleInfoParser());
          readers.Add(new GseSeriesMatrixReader());
        }
        return readers;
      }
    }
  }
}
