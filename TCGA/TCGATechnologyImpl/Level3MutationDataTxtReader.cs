using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class Level3MutationDataTxtReader : IFileReader<ExpressionData>
  {
    public Level3MutationDataTxtReader()
    {
    }

    public ExpressionData ReadFromFile(string fileName)
    {
      var result = new ExpressionData();
      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith("#"))
          {
            break;
          }
        }

        var headers = line.Split('\t').ToList();
        var nameindex = headers.IndexOf("Hugo_Symbol");
        var chromindex = headers.IndexOf("Chromosome");
        var startindex = headers.IndexOf("Start_position");
        var endindex = headers.IndexOf("End_position");
        var strandindex = headers.IndexOf("Strand");
        var classindex = headers.IndexOf("Variant_Classification");
        var variant = headers.IndexOf("Variant_Type");
        var tumorsampleindex = headers.IndexOf("Tumor_Sample_Barcode");

        //var refindex = headers.IndexOf("Reference_Allele");
        //var tur1index = headers.IndexOf("Tumor_Seq_Allele1");
        //var tur2index = headers.IndexOf("Tumor_Seq_Allele2");

        var nameindecies = new int[]{
          tumorsampleindex,
          nameindex,
          chromindex,
          startindex,
          endindex,
          strandindex,
          classindex,
          variant
        };

        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          var name = (from ind in nameindecies
                      select parts[ind]).Merge(":");
          var ev = new ExpressionValue(name, 1);
          result.Values.Add(ev);
        }
      }
      return result;
    }
  }
}
