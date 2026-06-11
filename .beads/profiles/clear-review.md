# C.L.E.A.R. Review Profile for Beads

## Purpose
Automatically run C.L.E.A.R. Framework review before closing any Beads ticket.

## Invocation
```bash
bd close <id> --clear-review
```

Or manually before closing:
```bash
bd clear-review
```

## Review Checklist

### C - Correctness
- [ ] Code compiles: `dotnet build Spacer.sln`
- [ ] Tests pass: `dotnet test --filter "FullyQualifiedName~<affected-project>"`
- [ ] Functional requirements from ticket met
- [ ] Edge cases handled (null, empty, boundaries, exceptions)
- [ ] No obvious logic errors (off-by-one, infinite loops, race conditions)

### L - Libraries & Dependencies
- [ ] All imports exist in .NET 10.0
- [ ] NuGet packages are valid and accessible
- [ ] API methods match current framework version
- [ ] No hallucinated types or methods
- [ ] Version compatibility verified

### E - Efficiency & Performance
- [ ] Algorithmic complexity appropriate for use case
- [ ] No redundant operations or loops
- [ ] Database queries minimized (no N+1)
- [ ] Resources properly disposed (`using` statements)
- [ ] No memory leaks or unmanaged resource issues

### A - Architecture Fit
- [ ] Follows Clean Architecture boundaries:
  - Domain: Pure business logic, no external dependencies
  - Application: Use cases, orchestration, DTOs
  - Infrastructure: External systems, persistence, implementations
  - Presentation: UI, input, rendering
- [ ] Dependency Injection used correctly
- [ ] Matches existing naming conventions:
  - Entities: Singular nouns (`Character`, `Faction`)
  - Services: `[Domain]Service` or `[Function]Resolver`
  - Repositories: `Csv[Entity]Repository`
- [ ] Code placed in correct layer
- [ ] Design patterns consistent with codebase

### R - Risks & Security
- [ ] No hardcoded secrets or credentials
- [ ] Input validation present (user input, file parsing, API calls)
- [ ] Proper error handling (no swallowed exceptions)
- [ ] No SQL injection vulnerabilities (parameterized queries)
- [ ] No XSS vulnerabilities (output encoding)
- [ ] Secure defaults (least privilege, fail-safe)

## Output Format

After review, output results in this format for the ticket comment:

```markdown
## C.L.E.A.R. Review Results

| Dimension | Status | Notes |
|-----------|--------|-------|
| **C**orrectness | ✓ / ⚠ / ✗ | [brief note] |
| **L**ibraries | ✓ / ⚠ / ✗ | [brief note] |
| **E**fficiency | ✓ / ⚠ / ✗ | [brief note] |
| **A**rchitecture | ✓ / ⚠ / ✗ | [brief note] |
| **R**isks | ✓ / ⚠ / ✗ | [brief note] |

### Summary
[Overall assessment and any follow-up actions]
```

## Automation

This profile is automatically invoked when:
1. Running `bd close <id>` (with confirmation)
2. Running `bd preflight` before session end
3. Manually invoking `bd clear-review`

## Integration with CLAUDE.md

See project documentation:
- `.claude/memory/clear-framework.md` — Full framework details
- `.claude/memory/session-completion.md` — Session completion protocol
- `CLAUDE.md` — Build & test commands, architecture overview
