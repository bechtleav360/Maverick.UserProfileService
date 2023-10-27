using System;
using System.Text.Json.Nodes;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     A JSON validation builder.
/// </summary>
public class ValidatorBuilder
{
    internal Type JsonNodeType { get; private set; } = typeof(JsonNode);

    /// <summary>
    ///     Sets the expected <see cref="JsonNode" /> type to the specified <typeparamref name="TJsonNode" />.
    /// </summary>
    /// <typeparam name="TJsonNode">The expected <see cref="JsonNode" /> type</typeparam>
    public void UseJsonNodeType<TJsonNode>()
        where TJsonNode : JsonNode
    {
        JsonNodeType = typeof(TJsonNode);
    }
}
