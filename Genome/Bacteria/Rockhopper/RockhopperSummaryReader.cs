using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperSummaryReader : IFileReader<RockhopperSummary>
  {
    const string MappingBlockStartKey = "Aligning sequencing reads from file:";

    public RockhopperSummary ReadFromFile(string fileName)
    {
      var result = new RockhopperSummary();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          //read individual file mapping result
          if (line.StartsWith(MappingBlockStartKey))
          {
            var mapping = new RockhopperMappingResult();
            result.MappingResults.Add(mapping);

            mapping.FileName = Path.GetFileNameWithoutExtension(line.StringAfter(MappingBlockStartKey).Trim());
            while ((line = sr.ReadLine()) != null)
            {
              line = line.Trim();

              if (line.Length == 0)
              {
                break;
              }

              if (line.StartsWith("Total reads:"))
              {
                mapping.TotalReads = line.StringAfter(":").Trim();
              }
              else if (line.StartsWith("Successfully aligned reads:"))
              {
                var parts = line.Split('\t');
                mapping.AlignedReads = parts[1];
                mapping.AlignedReadsPercentage = parts[2];
              }
              else if (line.StartsWith("Aligning (sense) to protein-coding genes:"))
              {
                mapping.ProteinReadsSense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (antisense) to protein-coding genes:"))
              {
                mapping.ProteinReadsAntisense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (sense) to ribosomal RNAs:"))
              {
                mapping.RibosomalRNAReadsSense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (antisense) to ribosomal RNAs:"))
              {
                mapping.RibosomalRNAReadsAntisense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (sense) to transfer RNAs:"))
              {
                mapping.TransferReadsSense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (antisense) to transfer RNAs:"))
              {
                mapping.TransferReadsAntisense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (sense) to miscellaneous RNAs:"))
              {
                mapping.MiscRNAReadsSense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning (antisense) to miscellaneous RNAs:"))
              {
                mapping.MiscRNAReadsAntisense = line.Split('\t')[1];
              }
              else if (line.StartsWith("Aligning to unannotated regions:"))
              {
                mapping.UnannotatedRead = line.Split('\t')[1];
              }
            }
          }
          else if (line.Trim().StartsWith("Number of differentially expressed protein coding genes:"))
          {
            result.DifferentialGenes = line.StringAfter(":").Trim();
          }
        }
      }

      return result;
    }
  }
}
