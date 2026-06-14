# C.L.E.A.R. Framework Review Skill

When invoked, apply the C.L.E.A.R. Framework systematically to review code or AI-generated artifacts.

## Invocation

Use this skill when:
- Reviewing newly generated code
- Assessing code changes before commit
- Evaluating AI-generated test cases
- Reviewing architecture proposals
- Checking requirements documents

## Review Protocol

### Step 1: Correctness
- [ ] Code compiles/runs without errors
- [ ] Functional requirements met
- [ ] Edge cases handled
- [ ] Logic is sound (no off-by-one, null issues)

### Step 2: Libraries & Dependencies
- [ ] All imports exist and are valid
- [ ] API methods match current versions
- [ ] No hallucinated packages
- [ ] Security vulnerabilities checked

### Step 3: Efficiency & Performance
- [ ] Algorithmic complexity appropriate
- [ ] No redundant operations
- [ ] Resources properly managed
- [ ] No obvious bottlenecks

### Step 4: Architecture Fit
- [ ] Follows existing patterns
- [ ] Respects layer boundaries
- [ ] Matches team standards
- [ ] Maintainable and extensible

### Step 5: Risks & Security
- [ ] No security vulnerabilities
- [ ] No hardcoded secrets
- [ ] Input validation present
- [ ] Secure defaults

## Output Format

```
## C.L.E.A.R. Review Results

**C - Correctness:** ✓ Pass / ⚠ Issues / ✗ Fail
- [Findings]

**L - Libraries:** ✓ Pass / ⚠ Issues / ✗ Fail
- [Findings]

**E - Efficiency:** ✓ Pass / ⚠ Issues / ✗ Fail
- [Findings]

**A - Architecture:** ✓ Pass / ⚠ Issues / ✗ Fail
- [Findings]

**R - Risks:** ✓ Pass / ⚠ Issues / ✗ Fail
- [Findings]

### Summary
[Overall assessment and recommended actions]
```
