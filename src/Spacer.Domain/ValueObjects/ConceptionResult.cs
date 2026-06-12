namespace Spacer.Domain.ValueObjects;

/// <summary>
/// Result of a conception attempt.
/// </summary>
public readonly record struct ConceptionResult(
    bool Success,
    bool Failed,
    bool Infertile,
    bool NoPartner,
    bool BlockedBySpecialFlag
)
{
    public static readonly ConceptionResult SuccessResult = new(true, false, false, false, false);
    public static readonly ConceptionResult FailedResult = new(false, true, false, false, false);
    public static readonly ConceptionResult InfertileResult = new(false, false, true, false, false);
    public static readonly ConceptionResult NoPartnerResult = new(false, false, false, true, false);
    public static readonly ConceptionResult BlockedResult = new(false, false, false, false, true);
}
