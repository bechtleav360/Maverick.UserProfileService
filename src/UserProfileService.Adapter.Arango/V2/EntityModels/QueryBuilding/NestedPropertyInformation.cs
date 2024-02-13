using System.Reflection;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents information about a nested property.
/// </summary>
public class NestedPropertyInformation
{
    internal bool IsList { get; set; }
    internal MethodInfo MethodToUse { get; set; }
    internal string PropertyName { get; set; }
}
