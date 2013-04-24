using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Sample;

namespace CQS.BreastCancer
{
  public enum ColumnName
  {
    Sample,
    Age,
    Stage,
    TumorStage,
    Grade,
    NodalStatus,
    PCR,
    DFS,
    DFSTime,
    RFS,
    RFSTime,
    DMFS,
    DMFSTime,
    OverallServive,
    DeadOfDisease,
    ER,
    PR,
    HER2
  }

  public class BreastCancerSampleItem : SampleItem
  {
    public BreastCancerSampleItem()
      : base()
    { }

    public BreastCancerSampleItem(string dataset, string filename)
      : base()
    {
      this.Dataset = dataset;
      this.Sample = filename;
    }

    public string Platform { get; set; }
    public string Patient { get; set; }
    public string Disease { get; set; }

    [SampleInfo]
    public string Age { get; set; }
    [SampleInfo]
    public string Sex { get; set; }
    [SampleInfo]
    public string Race { get; set; }
    [SampleInfo]
    public string Stage { get; set; }
    [SampleInfo]
    public string TumorStatus { get; set; }
    [SampleInfo]
    public string TumorSize { get; set; }
    [SampleInfo]
    public string Grade { get; set; }
    [SampleInfo]
    public string NodalStatus { get; set; }
    [SampleInfo]
    public string DFS { get; set; }
    [SampleInfo]
    public string DFSTime { get; set; }
    [SampleInfo]
    public string RFS { get; set; }
    [SampleInfo]
    public string RFSTime { get; set; }
    [SampleInfo]
    public string DMFS { get; set; }
    [SampleInfo]
    public string DMFSTime { get; set; }
    [SampleInfo]
    public string OverallSurvival { get; set; }
    [SampleInfo]
    public string OverallSurvivalTime { get; set; }
    [SampleInfo]
    public string Dead { get; set; }
    [SampleInfo]
    public string DeadOfDisease { get; set; }
    [SampleInfo]
    public string DiagnosisDate { get; set; }
    [SampleInfo]
    [StatusInfo]
    public string ER { get; set; }
    [SampleInfo]
    [StatusInfo]
    public string PR { get; set; }
    [SampleInfo]
    [StatusInfo]
    public string HER2 { get; set; }

    [SampleInfo]
    public string IntrinsicSubtype { get; set; }
    [SampleInfo]
    public string TimeToDeath { get; set; }

    [SampleInfo]
    public string P53 { get; set; }
    [SampleInfo]
    public string Histology { get; set; }
    [SampleInfo]
    public string MetastasisStatus { get; set; }
    [SampleInfo]
    public string MetastasisDate { get; set; }

    [SampleInfo]
    public string BoneMet { get; set; }

    [SampleInfo]
    public string BrainMet { get; set; }
    [SampleInfo]
    public string LiverMet { get; set; }
    [SampleInfo]
    public string LungMet { get; set; }

    [SampleInfo]
    public string FollowupTime { get; set; }

    [SampleInfo]
    public string HormonalTherapy { get; set; }
    [SampleInfo]
    public string ChemotherapyTreatment { get; set; }
    [SampleInfo]
    public string RadiationTreatment { get; set; }
    [SampleInfo]
    public string AliveAtEndpoint { get; set; }

    public string PCR { get; set; }
    public string Recurrence { get; set; }
    public string FamilyHistory { get; set; }
  }
}
