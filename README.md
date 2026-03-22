# schemata-validator-csharp

[![Test](https://github.com/nostrability/schemata-validator-csharp/actions/workflows/test.yml/badge.svg)](https://github.com/nostrability/schemata-validator-csharp/actions/workflows/test.yml)

C#/.NET validator for [Nostr](https://nostr.com/) JSON schemas. Uses [JsonSchema.Net](https://github.com/gregsdennis/json-everything).

## When to use this
JSON Schema validation is [not suited for runtime hot paths](https://github.com/nostrability/schemata#what-is-it-not-good-for). Use in **CI and integration tests**.

## License
GPL-3.0-or-later
