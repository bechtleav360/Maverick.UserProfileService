using System;

namespace UserProfileService.Projection.Abstractions.Annotations;

/// <summary>
///     Marks a property as a read-only property that cannot be set/updated directly via UpdateEntityAsync method of the
///     entity itself the property belongs to.
/// </summary>
public class ReadonlyAttribute : Attribute
{
}
