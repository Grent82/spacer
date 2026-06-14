# Spacer Bug Fix Template

Use this template for ALL bug fix tickets. Copy this exact structure.

---

## Bug Summary
**What:** One sentence describing the bug
**Where:** File path and line number
**Severity:** Low | Medium | High | Critical

## Root Cause
Explain exactly what causes the bug. Be specific about:
- Which code is wrong
- Why it is wrong
- What the compiler/runtime does instead

## Symptoms
What happens when this bug is triggered?
- Build errors?
- Runtime exceptions?
- Wrong behavior?
- Warning messages?

## Fix Instructions (STEP BY STEP)

### Step 1: Open the file
Open this file: `PATH_TO_FILE`

### Step 2: Locate the problematic code
Find this exact code (copy-paste to search):
```
PASTE_THE_BROKEN_CODE_HERE
```

### Step 3: Apply the fix
Replace the code above with this exact code:
```
PASTE_THE_FIXED_CODE_HERE
```

### Step 4: Build and verify
Run these commands:
```bash
dotnet build Spacer.sln
```

Expected output: Build succeeds with 0 errors

### Step 5: Verify the fix
Check that:
- [ ] The build succeeds
- [ ] The specific warning/error is gone
- [ ] No new warnings appeared
- [ ] The file compiles without errors

## Testing
**How to test:**
1. Step one to manually test
2. Step two to verify the fix

**Expected behavior after fix:**
- What should happen instead

## References
- Related files: `FILE1`, `FILE2`
- Related issues: `#ISSUE_ID`

---

## Example (for reference only)

## Bug Summary
**What:** NullReferenceException when accessing nullable property without checking HasValue
**Where:** src/Spacer.Application/Services/WeaponNameFormatter.cs, line 32
**Severity:** Medium

## Root Cause
The code checks `info is null` but then accesses `info.Value.Code`. If `info` is a nullable wrapper that has no value, accessing `.Value` throws NullReferenceException.

## Symptoms
- Runtime NullReferenceException when factionId does not exist in catalog
- Build warning CS8602 about possible null dereference

## Fix Instructions (STEP BY STEP)

### Step 1: Open the file
Open this file: `src/Spacer.Application/Services/WeaponNameFormatter.cs`

### Step 2: Locate the problematic code
Find this exact code:
```csharp
var info = _factionCatalog.FindById(factionId);
if (info is null || string.IsNullOrWhiteSpace(info.Value.Code))
{
    return $"F{factionId}";
}
return info.Value.Code.Trim();
```

### Step 3: Apply the fix
Replace the code above with:
```csharp
var info = _factionCatalog.FindById(factionId);
if (info is null || !info.HasValue || string.IsNullOrWhiteSpace(info.Value.Code))
{
    return $"F{factionId}";
}
return info.Value.Code.Trim();
```

### Step 4: Build and verify
Run:
```bash
dotnet build Spacer.sln
```

### Step 5: Verify the fix
- [ ] Build succeeds
- [ ] CS8602 warning is gone
- [ ] No new warnings

## Testing
**How to test:**
1. Run the game with a faction ID that does not exist
2. Verify no NullReferenceException is thrown

**Expected behavior:**
- Returns "F{factionId}" for missing factions instead of crashing
