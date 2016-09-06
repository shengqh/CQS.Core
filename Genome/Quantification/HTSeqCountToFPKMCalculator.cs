using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;
using System.Text.RegularExpressions;
using CQS.Genome.Mapping;

namespace CQS.Genome.Quantification
{
  public class HTSeqCountToFPKMCalculator : AbstractThreadProcessor
  {
    private HTSeqCountToFPKMCalculatorOptions options;

    public HTSeqCountToFPKMCalculator(HTSeqCountToFPKMCalculatorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      double[] sampleCounts;
      double[] geneLengths;

      var counts = CalculateFPKM(out sampleCounts, out geneLengths);

      new GeneCountTableFormat().WriteToFile(options.OutputFile, counts);

      var sampleCountFile = options.OutputFile + ".sampleReads";
      using (var sw = new StreamWriter(sampleCountFile))
      {
        sw.WriteLine("Sample\tReads");
        for (int i = 0; i < counts.Samples.Length; i++)
        {
          sw.WriteLine("{0}\t{1:0.###}", counts.Samples[i], sampleCounts[i]);
        }
      }

      var geneLengthFile = options.OutputFile + ".geneLength";
      using (var sw = new StreamWriter(geneLengthFile))
      {
        sw.WriteLine("Gene\tLength");
        for (int i = 0; i < counts.GeneValues.Count; i++)
        {
          sw.WriteLine("{0}\t{1:0.###}", counts.GeneValues[i][0], geneLengths[i]);
        }
      }

      return new[] { options.OutputFile };
    }

    public GeneCountTable CalculateFPKM(out double[] sampleCounts, out double[] geneLengths)
    {
      Progress.SetMessage("Reading gene length from {0} ...", options.GeneLengthFile);
      var columnNames = FileUtils.ReadColumnNames(options.GeneLengthFile);
      var lengthIndex = columnNames.ToList().FindIndex(m => m.ToLower().Equals("length"));
      if (lengthIndex < 0)
      {
        throw new Exception("Cannot find length column in file " + options.GeneLengthFile);
      }
      var geneLengthMap = new MapItemReader(0, lengthIndex).ReadFromFile(options.GeneLengthFile).ToDictionary(m => m.Key, m => double.Parse(m.Value.Value));

      Progress.SetMessage("Reading count table from {0} ...", options.InputFile);
      var counts = new GeneCountTableFormat().ReadFromFile(options.InputFile);

      if (!string.IsNullOrEmpty(options.KeyRegex))
      {
        var reg = new Regex(options.KeyRegex);
        geneLengthMap = geneLengthMap.ToDictionary(l => reg.Match(l.Key).Groups[1].Value, l => l.Value);
        counts.GeneValues[0][0] = reg.Match(counts.GeneValues[0][0]).Groups[1].Value;
      }

      Dictionary<string, double> sampleReads;
      if (File.Exists(options.SampleReadsFile))
      {
        Progress.SetMessage("Reading sample reads from {0} ...", options.SampleReadsFile);
        sampleReads = new MapItemReader(0, 1).ReadFromFile(options.SampleReadsFile).ToDictionary(m => m.Key, m => double.Parse(m.Value.Value));
      }
      else //use total mapped reads as total reads
      {
        sampleReads = new Dictionary<string, double>();
        for (int iSample = 0; iSample < counts.Samples.Length; iSample++)
        {
          double itotal = 0.0;
          for (int iGene = 0; iGene < counts.GeneValues.Count; iGene++)
          {
            itotal += counts.Count[iGene, iSample];
          }

          sampleReads[counts.Samples[iSample]] = itotal;
        }
      }

      foreach (var sample in counts.Samples)
      {
        if (!sampleReads.ContainsKey(sample))
        {
          throw new Exception(string.Format("No sample {0} found at sample reads file {1}", sample, options.SampleReadsFile));
        }
      }

      foreach (var geneValues in counts.GeneValues)
      {
        if (!geneLengthMap.ContainsKey(geneValues[0]))
        {
          throw new Exception(string.Format("No gene {0} found at gene length file {1}", geneValues[0], options.GeneLengthFile));
        }
      }

      sampleCounts = (from sample in counts.Samples
                          select sampleReads[sample]).ToArray();

      geneLengths = (from geneValues in counts.GeneValues
                     select geneLengthMap[geneValues[0]]).ToArray();

      for (int iGene = 0; iGene < geneLengths.Length; iGene++)
      {
        for (int iSample = 0; iSample < sampleCounts.Length; iSample++)
        {
          counts.Count[iGene, iSample] = counts.Count[iGene, iSample] * 1000000000 / (geneLengths[iGene] * sampleCounts[iSample]);
        }
      }
      return counts;
    }
  }
}
