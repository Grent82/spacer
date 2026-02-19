namespace Spacer.Domain.Rules;

public interface IDomainRule
{
    bool IsBroken();
    string Message { get; }
}
