# schemata-validator-csharp

[![Test](https://github.com/nostrability/schemata-validator-csharp/actions/workflows/test.yml/badge.svg)](https://github.com/nostrability/schemata-validator-csharp/actions/workflows/test.yml)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue?style=flat-square)](LICENSE)

C#/.NET validator for [Nostr](https://nostr.com/) protocol JSON schemas. Built on [`schemata-csharp`](https://github.com/nostrability/schemata-csharp) and [JsonSchema.Net](https://github.com/gregsdennis/json-everything).

## Overview

`schemata-validator-csharp` wraps the `schemata-csharp` embedded JSON Schema definitions with JsonSchema.Net validation, exposing ready-to-use static methods for common Nostr data structures. It validates Nostr events by kind.

Validation results separate hard errors (schema violations) from soft warnings (missing schemas for unknown kinds).

## When to use this

JSON Schema validation is [not suited for runtime hot paths](https://github.com/nostrability/schemata#what-is-it-not-good-for). Use this in:

- **CI pipelines** catching schema drift during builds
- **Integration tests** for clients and relays
- **xUnit / NUnit suites** verifying event construction correctness

## Installation

Add as a project reference (the package is not yet on NuGet):

```bash
git clone https://github.com/nostrability/schemata-csharp.git ../schemata-csharp
git clone https://github.com/nostrability/schemata-validator-csharp.git ../schemata-validator-csharp
dotnet add reference ../schemata-validator-csharp/SchemataValidator.csproj
```

Or add to your `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="../schemata-validator-csharp/SchemataValidator.csproj" />
</ItemGroup>
```

Requires .NET 8.0 and `JsonSchema.Net 7.*`.

## Quick Start

```csharp
using Nostrability.Schemata.Validator;

var hex64 = new string('a', 64);
var sig128 = new string('c', 128);
var eventJson = $$"""
{
    "id": "{{hex64}}",
    "pubkey": "{{new string('b', 64)}}",
    "created_at": 1700000000,
    "kind": 1,
    "tags": [],
    "content": "hello world",
    "sig": "{{sig128}}"
}
""";

var result = SchemataValidator.ValidateNote(eventJson);
Assert.True(result.Valid);
// result.Errors is empty, result.Warnings may flag unknown kinds
```

## API

All methods are static on the `SchemataValidator` class.

### `SchemataValidator.Validate(schemaJson, dataJson)`

```csharp
public static ValidationResult Validate(string schemaJson, string dataJson)
```

Low-level validator. Compiles a JSON Schema with JsonSchema.Net and validates `dataJson` against it. Use `ValidateNote` for the common case.

| Parameter | Type | Description |
|-----------|------|-------------|
| `schemaJson` | `string` | A JSON Schema document as a JSON string |
| `dataJson` | `string` | The data to validate as a JSON string |

### `SchemataValidator.ValidateNote(eventJson)`

```csharp
public static ValidationResult ValidateNote(string eventJson)
```

Validates a Nostr event against the schema for its `kind`. The schema is looked up from `schemata-csharp` using the key `kind{N}Schema`. Returns a warning (not an error) if no schema exists for the given kind.

| Parameter | Type | Description |
|-----------|------|-------------|
| `eventJson` | `string` | A Nostr event as a JSON string |

### `SchemataValidator.GetSchema(key)`

```csharp
public static JsonDocument? GetSchema(string key)
```

Looks up a schema by key from the `schemata-csharp` registry. Returns `null` if the key doesn't exist.

| Parameter | Type | Description |
|-----------|------|-------------|
| `key` | `string` | Schema registry key (e.g., `"kind1Schema"`, `"pTagSchema"`) |

### `ValidationResult`

```csharp
public record ValidationResult(
    bool Valid,
    List<ValidationError> Errors,
    List<ValidationError> Warnings
);
```

- `Valid` — `true` if the data passes all schema constraints
- `Errors` — schema violations; empty when `Valid` is `true`
- `Warnings` — unknown kind / missing schema alerts

### `ValidationError`

```csharp
public record ValidationError(
    string InstancePath = "",
    string Keyword = "",
    string Message = "",
    string SchemaPath = ""
);
```

### `Subject`

```csharp
public enum Subject { Relay, Client }
```

## Usage Examples

**Event validation:**

```csharp
var result = SchemataValidator.ValidateNote(
    "{\"id\":\"" + new string('a', 64) + "\",\"pubkey\":\"" + new string('b', 64) +
    "\",\"created_at\":1700000000,\"kind\":1,\"tags\":[],\"content\":\"hello\"," +
    "\"sig\":\"" + new string('c', 128) + "\"}"
);
Assert.True(result.Valid);
```

**Direct schema lookup:**

```csharp
var schema = SchemataValidator.GetSchema("kind1Schema");
Assert.NotNull(schema);
```

## Known Limitations

- **Partial kind coverage:** Only event kinds with a corresponding schema in `@nostrability/schemata` can be validated. `ValidateNote` returns a warning (not an error) when no schema exists for the given kind.
- **No `ValidateNip11` or `ValidateMessage`:** NIP-11 and protocol message validation are not yet implemented in this package.
- **No recursive content validation:** The `content` field of events containing stringified JSON (e.g., kind 0 metadata) is not recursively validated.
- **Alpha accuracy:** False positives and negatives are possible. The underlying schemas are in active development.

## Related Packages

- [`schemata-csharp`](https://github.com/nostrability/schemata-csharp) — C# data package containing embedded schemas and registry
- [`@nostrability/schemata`](https://github.com/nostrability/schemata) — canonical language-agnostic schema definitions
- [`@nostrwatch/schemata-js-ajv`](https://github.com/sandwichfarm/nostr-watch/tree/next/libraries/schemata-js-ajv) — JavaScript/TypeScript validator implementation

## License

[GPL-3.0-or-later](LICENSE)
