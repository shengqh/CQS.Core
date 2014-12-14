using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.TCGA
{
  public class TCGAClinicalInformationBuilder : AbstractThreadProcessor
  {
    private TCGAClinicalInformationBuilderOptions _options;

    private static string GetParticipant(string barcode)
    {
      return barcode.Substring(0, 12);
    }

    public TCGAClinicalInformationBuilder(TCGAClinicalInformationBuilderOptions options)
    {
      this._options = options;
      this._options.PrepareOptions();
    }

    private const string sampleBarcodeKey = "sample_barcode";

    public override IEnumerable<string> Process()
    {
      var format = new AnnotationFormat();

      var items = format.ReadFromFile(_options.ClinicalFile);
      format.Format.Headers = sampleBarcodeKey + "\t" + format.Format.Headers;

      var itemMap = items.ToDictionary(m => m.BarCode());

      using (StreamReader sr = new StreamReader(_options.DataFile))
      {
        var barcodes = sr.ReadLine().Split('\t').Where(m => m.StartsWith("TCGA")).ToList();
        List<Annotation> found = new List<Annotation>();
        foreach (var barcode in barcodes)
        {
          var patient = barcode.Substring(0, 12);

          Annotation ann;
          if (!itemMap.TryGetValue(patient, out ann))
          {
            if (_options.ThrowException)
            {
              throw new Exception("Cannot find patient information for " + patient);
            }

            Console.Error.WriteLine("Cannot find patient information for " + patient);
            ann = new Annotation();
          }

          var curann = new Annotation();
          curann.Annotations[sampleBarcodeKey] = barcode;
          curann.Annotations[TCGAClinicalInformation.BcrPatientBarcode] = patient;
          foreach (var e in ann.Annotations)
          {
            curann.Annotations[e.Key] = e.Value;
          }
          found.Add(curann);
        }
        format.WriteToFile(_options.OutputFile, found);
      }

      return new[] { _options.OutputFile };
    }
  }
}
