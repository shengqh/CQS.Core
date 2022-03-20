using RCPA;
using System.Collections.Generic;

namespace CQS.TCGA
{
  public static class TCGAClinicalInformation
  {
    public static string BcrPatientBarcode = "bcr_patient_barcode";
  }

  public static class TCGAClinicalInformationExtension
  {
    public static string BarCode(this IAnnotation ann)
    {
      object barcode;
      if (ann.Annotations.TryGetValue(TCGAClinicalInformation.BcrPatientBarcode, out barcode))
      {
        return barcode as string;
      }
      else
      {
        return string.Empty;
      }
    }

    public static string PatientCode(this IAnnotation ann)
    {
      string barcode = ann.BarCode();
      if (!string.IsNullOrEmpty(barcode))
      {
        return barcode.Substring(0, 12);
      }
      else
      {
        return string.Empty;
      }
    }
  }

  public class TCGAClinicalInformationFormat : AnnotationFormat
  {
    public override List<Annotation> ReadFromFile(string fileName)
    {
      var result = base.ReadFromFile(fileName);
      while (result.Count > 0 && !result[0].BarCode().StartsWith("TCGA"))
      {
        result.RemoveAt(0);
      }
      return result;
    }
  }
}
