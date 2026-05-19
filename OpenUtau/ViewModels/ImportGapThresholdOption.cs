namespace OpenUtau.App.ViewModels {
  public sealed class ImportGapThresholdOption {
    public int Denominator { get; init; }
    public string Label { get; init; } = "";

    public override string ToString() => Label;
  }
}
