namespace CopyWithValidation.Core;

/// <summary>Which stage of a copy is being reported.</summary>
// Values are in the order the phases happen – tests and saved reports rely on this
// (sorting an enum uses these numbers, and JSON saves enums as numbers by default).
// ToString() gives the name, e.g. CopyPhase.Copying.ToString() == "Copying".
public enum CopyPhase
{
    Copying = 0,
    Verifying = 1
}