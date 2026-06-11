# JsonElement API Usage Rules

## Do NOT use these methods (they don't exist):
- `TryGetBoolean()` — does not exist
- `TryGetString()` returns `string?`, not `bool`

## Correct patterns:

### Boolean check:
```csharp
if (value.ValueKind == JsonValueKind.True) {
    // value is true
}
if (value.ValueKind == JsonValueKind.False) {
    // value is false
}
```

### Integer check:
```csharp
if (value.TryGetInt32(out var intValue)) {
    // use intValue
}
```

### String check:
```csharp
var str = value.GetString(); // returns string? (null if not string)
if (!string.IsNullOrEmpty(str)) {
    // use str
}
```

### Conversion:
```csharp
var boolVal = value.GetBoolean(); // Direct call after ValueKind check
var intVal = value.GetInt32();    // Direct call after TryGetInt32 succeeds
var strVal = value.GetString();   // May return null
```

## Always check ValueKind first for primitives:
```csharp
switch (value.ValueKind) {
    case JsonValueKind.True:
    case JsonValueKind.False:
        // Boolean handling
    case JsonValueKind.Number:
        // Number handling
    case JsonValueKind.String:
        // String handling
}
```
