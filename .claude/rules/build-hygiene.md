# Build Hygiene Rules

## After every significant edit, build immediately:
```bash
dotnet build Spacer.sln
```

## Do NOT chain multiple edits without build:
- Edit one file → Build
- Edit second file → Build
- NOT: Edit 3 files → Build (harder to find the error)

## After modifying Rules/ValueObjects:
- Double-check property names before referencing them
- Copy-paste from source instead of typing
- Build immediately after adding new struct members

## When build fails:
- Fix the FIRST error only
- Rebuild
- Repeat (subsequent errors may be cascading)

## Why:
- Smaller error batches are easier to debug
- Prevents cascading errors from multiple sources
- Catches API misuse immediately
