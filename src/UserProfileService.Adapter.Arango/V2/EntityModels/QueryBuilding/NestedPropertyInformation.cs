using System.Reflection;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class NestedPropertyInformation
{
    internal bool IsList { get; set; }
    internal MethodInfo MethodToUse { get; set; }
    internal string PropertyName { get; set; }
}
