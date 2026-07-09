using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChipoBackend.API.ModelBinders;

/// <summary>
/// Convierte DateTime en bodies JSON a Kind=Utc (requerido por Npgsql para timestamptz).
/// </summary>
public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime());
}

/// <summary>
/// Convierte todo DateTime recibido por query string / route a Kind=Utc,
/// requerido por Npgsql para columnas timestamptz.
/// </summary>
public class UtcDateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None || string.IsNullOrWhiteSpace(valueResult.FirstValue))
        {
            if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
                bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (!DateTime.TryParse(valueResult.FirstValue, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Fecha inválida.");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(DateTime.SpecifyKind(parsed, DateTimeKind.Utc));
        return Task.CompletedTask;
    }
}

public class UtcDateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(DateTime) ? new UtcDateTimeModelBinder() : null;
    }
}
