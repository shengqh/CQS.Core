using System.Collections.Generic;
using System.Linq;

namespace CQS.TCGA
{
  public static class TCGAReader
  {
    public static ExpressionMatrix ReadMatrix(this TCGATechnologyType ttt, TCGADataType tdt, List<BarInfo> bis, List<string> genes)
    {
      double?[,] data = new double?[genes.Count, bis.Count];
      for (int i = 0; i < bis.Count; i++)
      {
        var reader = ttt.GetTechnology().GetReader();
        var fn = tdt == TCGADataType.Count ? ttt.GetTechnology().GetCountFilename(bis[i].FileName) : bis[i].FileName;
        var dd = reader.ReadFromFile(fn).Values.ToDictionary(m => m.Name, m => m.Value);
        for (int j = 0; j < genes.Count; j++)
        {
          if (dd.ContainsKey(genes[j]))
          {
            data[j, i] = dd[genes[j]];
          }
          else
          {
            data[j, i] = null;
          }
        }
      }

      ExpressionMatrix result = new ExpressionMatrix();
      result.Values = data;
      result.Rownames = genes.ToArray();
      result.Colnames = bis.ConvertAll(m => m.BarCode).ToArray();
      return result;
    }
  }
}
