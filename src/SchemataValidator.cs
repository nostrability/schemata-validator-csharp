using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Nostrability.Schemata;

namespace Nostrability.Schemata.Validator;

public record ValidationError(string InstancePath = "", string Keyword = "", string Message = "", string SchemaPath = "");
public record ValidationResult(bool Valid, List<ValidationError> Errors, List<ValidationError> Warnings);
public enum Subject { Relay, Client }

public static class SchemataValidator
{
    public static ValidationResult ValidateNote(string eventJson)
    {
        using var doc = JsonDocument.Parse(eventJson);
        if (!doc.RootElement.TryGetProperty("kind", out var kindEl))
            return new(false, new() { new("", "note", "Event missing 'kind' field") }, new());
        var kind = kindEl.GetInt32();
        var schema = Schemata.Get($"kind{kind}Schema");
        if (schema == null)
            return new(false, new(), new() { new("", "note", $"No schema found for kind {kind}") });
        return Validate(schema.RootElement.GetRawText(), eventJson);
    }

    public static ValidationResult Validate(string schemaJson, string dataJson)
    {
        try
        {
            var schema = JsonSchema.FromText(schemaJson);
            var data = JsonNode.Parse(dataJson);
            var result = schema.Evaluate(data, new EvaluationOptions { OutputFormat = OutputFormat.List });
            if (result.IsValid) return new(true, new(), new());
            var errors = result.Details?
                .Where(d => !d.IsValid && d.Errors != null)
                .SelectMany(d => d.Errors!.Select(e => new ValidationError(d.InstanceLocation?.ToString() ?? "", e.Key, e.Value, d.EvaluationPath?.ToString() ?? "")))
                .ToList() ?? new();
            return new(false, errors, new());
        }
        catch (Exception e)
        {
            return new(false, new() { new("", "compilation", e.Message) }, new());
        }
    }

    public static JsonDocument? GetSchema(string key) => Schemata.Get(key);
}
