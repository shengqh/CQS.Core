using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.ChipSeq
{
  public class ChipSeqItemFormat : IFileReader<List<ChipSeqItem>>
  {
    public List<ChipSeqItem> ReadFromFile(string fileName)
    {
      var result = new List<ChipSeqItem>();

      var fn = Path.GetFileNameWithoutExtension(fileName);

      using (StreamReader sr = new StreamReader(fileName) )
      {
        string line;

        //ignore the comment lines
        while ((line = sr.ReadLine()) != null)
        {
          if(!line.StartsWith("#"))
          {
            break;
          }
        }
        
        //ignore the header line and read data
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if(parts.Length < 11){
            Console.WriteLine("File = {0}\nLine = {1}", fileName, line);
            break;
          }

          var item = new ChipSeqItem();
          try
          {
            item.Filename = fn;
            item.Chromosome = parts[0];
            item.Start = int.Parse(parts[1]);
            item.End = int.Parse(parts[2]);
            item.ReadDensity = double.Parse(parts[3]);
            item.TreatmentCount = double.Parse(parts[4]);
            item.ControlCount = double.Parse(parts[5]);
            item.EnrichmentFactor = MyConvert.ToDouble(parts[6], 0);
            item.GeneSymbol = parts[7];
            item.LongestTranscript = parts[8];
            item.OverlapType = parts[9];

            item.DistanceToTSS = MyConvert.ToInt(parts[10], 0);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message + "\n" + line);
            throw new Exception("Parsing ChipSeqItem error: " + ex.Message + "\n line:" + line);
          }
          result.Add(item);
        }
      }

      return result;
    }
  }
}
