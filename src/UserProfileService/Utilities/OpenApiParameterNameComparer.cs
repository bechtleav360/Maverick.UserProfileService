using Microsoft.OpenApi.Models;

namespace UserProfileService.Utilities;

internal class OpenApiParameterNameComparer : IEqualityComparer<OpenApiParameter>
{
    public bool Equals(OpenApiParameter x, OpenApiParameter y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Name != null && y.Name != null && x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(OpenApiParameter obj)
    {
        return obj.Name != null ? obj.Name.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0;
    }
}
