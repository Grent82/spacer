namespace Spacer.Application.Services;

using System;
using System.Collections.Generic;
using Spacer.Domain.Entities;

public sealed record CharacterLifecycleResult(
    IReadOnlyList<Character> Births,
    IReadOnlyList<Character> Deaths
)
{
    public static readonly CharacterLifecycleResult Empty =
        new(Array.Empty<Character>(), Array.Empty<Character>());
}
