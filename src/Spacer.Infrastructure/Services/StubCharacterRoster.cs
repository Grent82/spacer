namespace Spacer.Infrastructure.Services;

using System;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;

public sealed class StubCharacterRoster : ICharacterRoster
{
    public IReadOnlyList<Character> GetAll() => Array.Empty<Character>();
}
