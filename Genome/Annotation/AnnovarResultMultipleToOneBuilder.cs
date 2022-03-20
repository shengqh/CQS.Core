using CQS.Genome.SomaticMutation;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Annotation
{
  public class AnnovarResultMultipleToOneBuilder : AbstractThreadProcessor
  {
    private readonly AnnovarResultMultipleToOneBuilderOptions _options;

    public AnnovarResultMultipleToOneBuilder(AnnovarResultMultipleToOneBuilderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      var files = _options.GetAnnovarFiles();
      var filelist = files.Keys.ToArray();

      using (var sw = new StreamWriter(_options.OutputFile))
      {
        //deal with comments
        using (var sr = new StreamReader(filelist[0]))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith("##MuTect="))
            {
              sw.WriteLine(line);
              for (var i = 1; i < filelist.Length; i++)
              {
                using (var sr2 = new StreamReader(filelist[i]))
                {
                  while ((line = sr2.ReadLine()) != null)
                  {
                    if (!line.StartsWith("##MuTect="))
                      continue;

                    sw.WriteLine(line);
                    break;
                  }
                }
              }
            }
            else if (!line.StartsWith("#"))
            {
              break;
            }
            else
            {
              sw.WriteLine(line);
            }
          }
        }

        //deal with data
        var data = new List<FileData>();
        foreach (var file in filelist)
        {
          var lines = File.ReadAllLines(file);
          var mutect = lines.FirstOrDefault(m => m.StartsWith("##MuTect="));
          string normal, tumor, normalName, tumorName;
          if (mutect != null)
          {
            normal = mutect.StringAfter("normal_sample_name=").StringBefore(" ");
            tumor = mutect.StringAfter("tumor_sample_name=").StringBefore(" ");
            normalName = normal;
            tumorName = tumor;
          }
          else
          {
            normal = "NORMAL";
            tumor = "TUMOR";
            normalName = Path.GetFileName(file).StringBefore(".") + "_normal";
            tumorName = Path.GetFileName(file).StringBefore(".") + "_tumor";
          }
          var header = lines.First(m => !m.StartsWith("#"));
          var headers = header.Split('\t');
          var infoIndex = Array.IndexOf(headers, "INFO");
          var formatIndex = Array.IndexOf(headers, "FORMAT");
          var normalIndex = Array.IndexOf(headers, normal);
          var tumorIndex = Array.IndexOf(headers, tumor);
          var dictionary = new Dictionary<string, FileDataValue>();
          foreach (var line in lines)
          {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("Chr"))
              continue;

            var parts = line.Split('\t');
            if (parts.Length != headers.Length)
              continue;

            var vnormal = GetAllele(parts, normalIndex);
            var vtumor = GetAllele(parts, tumorIndex);
            var value = new FileDataValue()
            {
              Key = parts[0] + "_" + parts[1],
              Parts = parts,
              VNormal = vnormal,
              VTumor = vtumor
            };
            dictionary.Add(value.Key, value);
          }
          data.Add(new FileData()
          {
            File = file,
            Normal = normalName,
            Tumor = tumorName,
            Headers = headers,
            InfoIndex = infoIndex,
            FormatIndex = formatIndex,
            NormalIndex = normalIndex,
            TumorIndex = tumorIndex,
            Data = dictionary
          });
        }

        //get all positions
        var keys = (from d in data
                    from k in d.Data.Keys
                    select k).Distinct().ToList().ConvertAll(m =>
                    {
                      var p = m.Split('_');
                      return new
                      {
                        Key = m,
                        Chr = p[0],
                        Position = int.Parse(p[1])
                      };
                    }
          );

        GenomeUtils.SortChromosome(keys, m => m.Chr, m => m.Position);

        var keyMap = keys.ToDictionary(m => m.Key);

        //check by original vcf file to fill the other columns
        foreach (var d in data)
        {
          var vcf = files[d.File];
          if (string.IsNullOrEmpty(vcf))
            continue;

          var vd = d.Data;

          using (var sr = new StreamReader(vcf))
          {
            string line;
            var normalIndex = -1;
            var tumorIndex = -1;
            while ((line = sr.ReadLine()) != null)
            {
              if (!line.StartsWith("#CHROM"))
                continue;

              var parts = line.Split('\t');
              normalIndex = Array.IndexOf(parts, d.Normal);
              tumorIndex = Array.IndexOf(parts, d.Tumor);
              break;
            }
            if (normalIndex == -1)
            {
              throw new Exception(string.Format("Normal {0} is not included in detail vcf file {1} but in annovar result {1}", d.Normal, vcf, d.File));
            }
            if (tumorIndex == -1)
            {
              throw new Exception(string.Format("Tumor {0} is not included in detail vcf file {1} but in annovar result {1}", d.Tumor, vcf, d.File));
            }

            var minIndex = Math.Max(normalIndex, tumorIndex) + 1;

            while ((line = sr.ReadLine()) != null)
            {
              var parts = line.Split('\t');
              if (parts.Length < minIndex)
              {
                break;
              }

              var key = parts[0] + "_" + parts[1];
              if (!keyMap.ContainsKey(key))
                continue;

              FileDataValue fdv;
              if (!vd.TryGetValue(key, out fdv))
              {
                fdv = new FileDataValue()
                {
                  Key = key,
                  Parts = null
                };
                vd[key] = fdv;
              }
              fdv.VNormal = GetAllele(parts, normalIndex);
              fdv.VTumor = GetAllele(parts, tumorIndex);
            }
          }
        }

        //write header
        for (var i = 0; i < data[0].Headers.Length; i++)
        {
          if (i == data[0].NormalIndex || i == data[0].TumorIndex || i == data[0].InfoIndex || i == data[0].FormatIndex)
          {
            continue;
          }
          else
          {
            if (i != 0)
            {
              sw.Write("\t");
            }
            sw.Write(data[0].Headers[i]);
          }
        }

        var normalnames = (from d in data
                           select d.Normal).Distinct().ToArray();
        sw.Write("\t{0}", normalnames.Merge('\t'));
        var tumornames = (from d in data
                          select d.Tumor).Distinct().ToArray();
        sw.WriteLine("\t{0}", tumornames.Merge('\t'));

        foreach (var key in keys)
        {
          var d1 = data.First(d => d.Data.ContainsKey(key.Key) && d.Data[key.Key].Parts != null);
          var v1 = d1.Data[key.Key];
          for (var i = 0; i < v1.Parts.Length; i++)
          {
            if (i == 0)
            {
              sw.Write("{0}", v1.Parts[0]);
            }
            else if (i == d1.InfoIndex || i == d1.FormatIndex || i == d1.NormalIndex || i == d1.TumorIndex)
            {
              continue;
            }
            else
            {
              sw.Write("\t{0}", v1.Parts[i]);
            }
          }

          foreach (var name in normalnames)
          {
            var dn = (from d in data
                      where d.Normal.Equals(name) && d.Data.ContainsKey(key.Key)
                      select d).FirstOrDefault();
            if (dn == null)
            {
              sw.Write("\t");
            }
            else
            {
              var vn = dn.Data[key.Key].VNormal;
              sw.Write("\t{0}", vn);
            }
          }

          foreach (var name in tumornames)
          {
            var dn = (from d in data
                      where d.Tumor.Equals(name) && d.Data.ContainsKey(key.Key)
                      select d).FirstOrDefault();
            if (dn == null)
            {
              sw.Write("\t");
            }
            else
            {
              var vn = dn.Data[key.Key].VTumor;
              sw.Write("\t{0}", vn);
            }
          }
          sw.WriteLine();
        }
      }

      return new[] { _options.OutputFile };
    }

    private static string GetAllele(string[] parts, int normalIndex)
    {
      var snormal = parts[normalIndex];
      var mnormal = SomaticMutationUtils.MutectPattern.Match(snormal);
      var vnormal = mnormal.Groups[1].Value + " , " + mnormal.Groups[2].Value;
      return vnormal;
    }
  }
}
