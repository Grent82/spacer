## Bug Summary
**What:** NullReferenceException when faction catalog FindById returns a nullable with no value
**Where:** src/Spacer.Application/Services/WeaponNameFormatter.cs, line 32
**Severity:** Medium

## Root Cause
The code at line 32 checks if info is null but then immediately accesses info.Value.Code. The FindById method returns a nullable wrapper type. When the faction is not found, info.HasValue is false, and accessing info.Value throws NullReferenceException.

The check "info is null" does NOT protect against info.HasValue == false.

## Symptoms
- Runtime NullReferenceException when a factionId is not found in the catalog
- Build may show CS8602 warning about possible null reference

## Fix Instructions (STEP BY STEP)

### Step 1: Open the file
Open this file: src/Spacer.Application/Services/WeaponNameFormatter.cs

### Step 2: Locate the problematic code
Go to line 29-38. Find the ResolveFactionCode method.

### Step 3: Apply the fix
Replace line 32. Change this code:
if (info is null || string.IsNullOrWhiteSpace(info.Value.Code))

To this code:
if (info is null || !info.HasValue || string.IsNullOrWhiteSpace(info.Value.Code))

### Step 4: Build and verify
Run: dotnet build Spacer.sln

Expected output: Build succeeds with 0 errors and no CS8602 warning on line 32

### Step 5: Verify the fix
Check that:
- The build succeeds
- The CS8602 warning is gone
- No new warnings appeared in WeaponNameFormatter.cs
- The file compiles without errors

## Testing
**How to test:**
1. Run the game
2. Call BuildName() or BuildImageKey() with a factionId that does not exist in the catalog
3. Verify no NullReferenceException is thrown

**Expected behavior after fix:**
- Returns "F{factionId}" (e.g., "F999") for missing factions instead of crashing
- No exception when info.HasValue is false

## References
- Related file: src/Spacer.Application/Ports/IFactionCatalog.cs (defines FindById return type)
- Build hygiene: Run dotnet build after every change
