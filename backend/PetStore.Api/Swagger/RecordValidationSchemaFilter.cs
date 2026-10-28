using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PetStore.Api.Swagger;

// Positional records carry their validation attributes on constructor parameters
// (where MVC requires them), but Swashbuckle only reads properties. Copy them across.
public class RecordValidationSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema target || target.Properties is not { Count: > 0 })
            return;

        var constructor = context.Type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        var parameters = constructor?.GetParameters();
        if (parameters is not { Length: > 0 })
            return;

        foreach (var parameter in parameters)
        {
            var attributes = parameter.GetCustomAttributes<ValidationAttribute>().ToList();
            if (attributes.Count == 0)
                continue;

            var propertyName = target.Properties.Keys
                .FirstOrDefault(k => string.Equals(k, parameter.Name, StringComparison.OrdinalIgnoreCase));
            if (propertyName is null)
                continue;

            // Referenced schemas (enums, nested objects) are shared components and can't be mutated here.
            var property = target.Properties[propertyName] as OpenApiSchema;

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case RequiredAttribute:
                        target.Required ??= new HashSet<string>();
                        target.Required.Add(propertyName);
                        if (property is { Type: not null })
                            property.Type &= ~JsonSchemaType.Null;
                        break;

                    case EmailAddressAttribute when property is not null:
                        property.Format = "email";
                        break;

                    // On collections these mean item count, which JSON Schema expresses as minItems/maxItems.
                    case MinLengthAttribute minLength when property is not null:
                        if (IsArray(property))
                            property.MinItems = minLength.Length;
                        else
                            property.MinLength = minLength.Length;
                        break;

                    case MaxLengthAttribute maxLength when property is not null:
                        if (IsArray(property))
                            property.MaxItems = maxLength.Length;
                        else
                            property.MaxLength = maxLength.Length;
                        break;

                    case StringLengthAttribute stringLength when property is not null:
                        if (stringLength.MinimumLength > 0)
                            property.MinLength = stringLength.MinimumLength;
                        property.MaxLength = stringLength.MaximumLength;
                        break;

                    case RangeAttribute range when property is not null:
                        property.Minimum = Convert.ToString(range.Minimum, CultureInfo.InvariantCulture);
                        property.Maximum = Convert.ToString(range.Maximum, CultureInfo.InvariantCulture);
                        break;
                }
            }
        }
    }

    private static bool IsArray(OpenApiSchema schema) =>
        schema.Type is { } type && type.HasFlag(JsonSchemaType.Array);
}
