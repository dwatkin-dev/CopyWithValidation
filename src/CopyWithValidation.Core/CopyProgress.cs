namespace CopyWithValidation.Core;

/// <summary>One progress report: which phase we are in, and how far through it (0–100).</summary>
// A positional record: this one line generates a constructor, two read-only properties
// (Phase, Percent), a readable ToString(), and VALUE equality – two records with the same
// Phase and Percent are Equal, even if they are separate objects (a class compares references).
public record CopyProgress(CopyPhase Phase, double Percent);
