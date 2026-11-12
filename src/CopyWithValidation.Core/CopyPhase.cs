namespace CopyWithValidation.Core;

// Values are in the order the phases happen – tests and saved reports rely on this
public enum CopyPhase
{
    Copying = 0,
    Verifying = 1
}