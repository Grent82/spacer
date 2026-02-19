namespace Spacer.Domain.Rules;

using System;

public sealed class DomainRuleException : Exception
{
    public DomainRuleException(string message) : base(message)
    {
    }
}
