namespace Spacer.Infrastructure.Config;

using Spacer.Application.Config;
using Spacer.Application.Ports;
using Spacer.Domain.Enums;

public sealed class StubJudgementLookup : IJudgementLookup
{
    private readonly JudgementTable _table;

    public StubJudgementLookup(JudgementTable? table = null)
    {
        _table = table ?? JudgementTable.Empty;
    }

    public int GetJudgementValue(int category, PersonalityType personality)
    {
        return _table.GetValue(category, personality);
    }
}
