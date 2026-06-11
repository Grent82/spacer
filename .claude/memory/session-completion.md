---
name: session-completion-protocol
description: Mandatory steps before closing a Beads ticket and ending a work session
metadata:
  type: feedback
---

# Session Completion Protocol

**Before `bd close <id>`, complete ALL steps below:**

## 1. Quality Gates
- [ ] Build succeeds: `dotnet build Spacer.sln`
- [ ] Tests pass: `dotnet test`
- [ ] Lint/Format check (if applicable)

## 2. C.L.E.A.R. Review
Run a C.L.E.A.R. review on all changed code and add to ticket comment:

```markdown
## C.L.E.A.R. Review
- **C**orrectness: ✓ / ⚠ / ✗ — [brief note]
- **L**ibraries: ✓ / ⚠ / ✗ — [brief note]
- **E**fficiency: ✓ / ⚠ / ✗ — [brief note]
- **A**rchitecture: ✓ / ⚠ / ✗ — [brief note]
- **R**isks: ✓ / ⚠ / ✗ — [brief note]
```

## 3. Create Follow-up Issues
- [ ] Any remaining work → new Beads issue
- [ ] Technical debt noted → new Beads issue

## 4. Push to Remote
```bash
git pull --rebase
bd dolt push
git push
git status  # MUST show "up to date with origin"
```

## 5. Cleanup
- [ ] Clear stashes: `git stash clear`
- [ ] Prune branches: `git remote prune origin`

## 6. Close Ticket
```bash
bd close <id>
```

## 7. Hand-off Context
Provide summary for next session:
- What was done
- What remains
- Any blockers or decisions needed

---

**CRITICAL:** Work is NOT complete until `git push` succeeds. Never say "ready to push" — YOU must push.
