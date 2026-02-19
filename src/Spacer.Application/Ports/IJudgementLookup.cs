namespace Spacer.Application.Ports;

using Spacer.Domain.Enums;

public interface IJudgementLookup
{
    int GetJudgementValue(int category, PersonalityType personality);
}
