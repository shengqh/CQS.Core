using System.Collections.Generic;
using System.Linq;

namespace CQS.TCGA
{
  public class TCGASampleCode
  {
    private static Dictionary<int, TCGASampleCode> _codeMap = new Dictionary<int, TCGASampleCode>();

    private TCGASampleCode(int code, string definition, string shortLetterCode)
    {
      this.Code = code;
      this.Definition = definition;
      this.ShortLetterCode = shortLetterCode;

      _codeMap[code] = this;
    }

    public int Code { get; private set; }

    public string Definition { get; private set; }

    public string ShortLetterCode { get; private set; }

    public TCGASampleType SampleType
    {
      get
      {
        if (this.Code == ControlAnalyte.Code)
        {
          return TCGASampleType.Control;
        }

        switch (this.ShortLetterCode[0])
        {
          case 'T': return TCGASampleType.Tumor;
          case 'N': return TCGASampleType.Normal;
          default: return TCGASampleType.Other;
        }
      }
    }

    public static TCGASampleCode Find(int code)
    {
      if (_codeMap.ContainsKey(code))
      {
        return _codeMap[code];
      }
      else
      {
        return null;
      }
    }

    public static TCGASampleCode Find(string shortCode)
    {
      var upper = shortCode.ToUpper();
      foreach (var code in _codeMap.Values)
      {
        if (code.ShortLetterCode.Equals(upper))
        {
          return code;
        }
      }

      return null;
    }

    public static TCGASampleCode PrimarySolidTumor = new TCGASampleCode(01, "Primary solid Tumor", "TP");
    public static TCGASampleCode RecurrentSolidTumor = new TCGASampleCode(02, "Recurrent Solid Tumor", "TR");
    public static TCGASampleCode PrimaryBloodDerivedCancer_PeripheralBlood = new TCGASampleCode(03, "Primary Blood Derived Cancer - Peripheral Blood", "TB");
    public static TCGASampleCode RecurrentBloodDerivedCancer_BoneMarrow = new TCGASampleCode(04, "Recurrent Blood Derived Cancer - Bone Marrow", "TRBM");
    public static TCGASampleCode Additional_NewPrimary = new TCGASampleCode(05, "Additional - New Primary", "TAP");
    public static TCGASampleCode Metastatic = new TCGASampleCode(06, "Metastatic", "TM");
    public static TCGASampleCode AdditionalMetastatic = new TCGASampleCode(07, "Additional Metastatic", "TAM");
    public static TCGASampleCode HumanTumorOriginalCells = new TCGASampleCode(08, "Human Tumor Original Cells", "THOC");
    public static TCGASampleCode PrimaryBloodDerivedCancer_BoneMarrow = new TCGASampleCode(09, "Primary Blood Derived Cancer - Bone Marrow", "TBM");
    public static TCGASampleCode BloodDerivedNormal = new TCGASampleCode(10, "Blood Derived Normal", "NB");
    public static TCGASampleCode SolidTissueNormal = new TCGASampleCode(11, "Solid Tissue Normal", "NT");
    public static TCGASampleCode BuccalCellNormal = new TCGASampleCode(12, "Buccal Cell Normal", "NBC");
    public static TCGASampleCode EBVImmortalizedNormal = new TCGASampleCode(13, "EBV Immortalized Normal", "NEBV");
    public static TCGASampleCode BoneMarrowNormal = new TCGASampleCode(14, "Bone Marrow Normal", "NBM");
    public static TCGASampleCode ControlAnalyte = new TCGASampleCode(20, "Control Analyte", "CELLC");
    public static TCGASampleCode RecurrentBloodDerivedCancer_PeripheralBlood = new TCGASampleCode(40, "Recurrent Blood Derived Cancer - Peripheral Blood", "TRB");
    public static TCGASampleCode CellLines = new TCGASampleCode(50, "Cell Lines", "CELL");
    public static TCGASampleCode PrimaryXenograftTissue = new TCGASampleCode(60, "Primary Xenograft Tissue", "XP");
    public static TCGASampleCode CellLineDerivedXenograftTissue = new TCGASampleCode(61, "Cell Line Derived Xenograft Tissue", "XCL");

    public static TCGASampleCode[] GetSampleCodes()
    {
      return _codeMap.Values.ToArray();
    }
  }
}
