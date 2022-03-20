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

    [SampleInfoAttribute]
    public string Age { get; set; }
    [SampleInfoAttribute]
    public string Sex { get; set; }
    [SampleInfoAttribute]
    public string Race { get; set; }
    [SampleInfoAttribute]
    public string Stage { get; set; }
    [SampleInfoAttribute]
    public string DukeStage { get; set; }
    [SampleInfoAttribute]
    public string TumorStatus { get; set; }
    [SampleInfoAttribute]
    public string TumorSize { get; set; }
    [SampleInfoAttribute]
    public string Grade { get; set; }
    [SampleInfoAttribute]
    public string NodalStatus { get; set; }
    [SampleInfoAttribute]
    public string DFS { get; set; }
    [SampleInfoAttribute]
    public string DFSTime { get; set; }
    [SampleInfoAttribute]
    public string RFS { get; set; }
    [SampleInfoAttribute]
    public string RFSTime { get; set; }
    [SampleInfoAttribute]
    public string DMFS { get; set; }
    [SampleInfoAttribute]
    public string DMFSTime { get; set; }
    [SampleInfoAttribute]
    public string OverallSurvival { get; set; }
    [SampleInfoAttribute]
    public string OverallSurvivalTime { get; set; }
    [SampleInfoAttribute]
    public string Dead { get; set; }
    [SampleInfoAttribute]
    public string DeadOfDisease { get; set; }
    [SampleInfoAttribute]
    public string DiagnosisDate { get; set; }
    [SampleInfoAttribute]
    [StatusInfoAttribute]
    public string ER { get; set; }
    [SampleInfoAttribute]
    [StatusInfoAttribute]
    public string PR { get; set; }
    [SampleInfoAttribute]
    [StatusInfoAttribute]
    public string HER2 { get; set; }

    [SampleInfoAttribute]
    public string IntrinsicSubtype { get; set; }
    [SampleInfoAttribute]
    public string TimeToDeath { get; set; }

    [SampleInfoAttribute]
    public string P53 { get; set; }
    [SampleInfoAttribute]
    public string Histology { get; set; }
    [SampleInfoAttribute]
    public string MetastasisStatus { get; set; }
    [SampleInfoAttribute]
    public string MetastasisDate { get; set; }

    [SampleInfoAttribute]
    public string BoneMet { get; set; }

    [SampleInfoAttribute]
    public string BrainMet { get; set; }
    [SampleInfoAttribute]
    public string LiverMet { get; set; }
    [SampleInfoAttribute]
    public string LungMet { get; set; }

    [SampleInfoAttribute]
    public string FollowupTime { get; set; }

    [SampleInfoAttribute]
    public string HormonalTherapy { get; set; }
    [SampleInfoAttribute]
    public string ChemotherapyTreatment { get; set; }
    [SampleInfoAttribute]
    public string RadiationTreatment { get; set; }
    [SampleInfoAttribute]
    public string AliveAtEndpoint { get; set; }

    public string PCR { get; set; }
    public string Recurrence { get; set; }
    public string FamilyHistory { get; set; }

    [SampleInfoAttribute]
    public string Comment { get; set; }
  }
}
