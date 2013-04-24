using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA.Microarray
{
  public class Level3MicroarrayDataTxtReader : IFileReader<ExpressionData>
  {
    private ExpressionDataRawReader reader;

    public Level3MicroarrayDataTxtReader()
    {
      this.reader = new ExpressionDataRawReader(2, 1, 2);
    }

    public ExpressionData ReadFromFile(string fileName)
    {
      ExpressionData result = reader.ReadFromFile(fileName);

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line = sr.ReadLine();
        line = sr.ReadLine();

        result.IsLog2Value = line.Contains("log2");
      }
      return result;
    }
  }
}
