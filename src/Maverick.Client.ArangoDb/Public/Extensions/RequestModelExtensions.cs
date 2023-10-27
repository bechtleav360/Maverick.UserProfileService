using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Contains methods to modify request models.
/// </summary>
internal static class RequestModelExtensions
{
    /// <summary>
    ///     Tries to set the <c>Count</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetCountParameter(
        this Request<CreateCursorBody> createCursorRequest,
        Dictionary<string, object> parameters)
    {
        return createCursorRequest?.BodyObject != null
            && createCursorRequest.BodyObject.TrySetCountParameter(parameters);
    }

    /// <summary>
    ///     Tries to set the <c>Count</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetCountParameter(
        this CreateCursorBody createCursorRequest,
        Dictionary<string, object> parameters)
    {
        if (createCursorRequest == null
            || parameters == null
            || createCursorRequest.Count.HasValue
            || !parameters.TryGetValue(ParameterName.Count, out object raw)
            || !(raw is bool count))
        {
            return false;
        }

        createCursorRequest.Count = count;

        return true;
    }

    /// <summary>
    ///     Tries to set the <c>TTL</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetTTlParameter(
        this Request<CreateCursorBody> createCursorRequest,
        Dictionary<string, object> parameters)
    {
        return createCursorRequest?.BodyObject != null
            && createCursorRequest.BodyObject.TrySetTTlParameter(parameters);
    }

    /// <summary>
    ///     Tries to set the <c>TTL</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetTTlParameter(
        this CreateCursorBody createCursorRequest,
        Dictionary<string, object> parameters)
    {
        if (createCursorRequest == null
            || parameters == null
            || createCursorRequest.Ttl.HasValue
            || !parameters.TryGetValue(ParameterName.Ttl, out object raw)
            || !(raw is int ttl))
        {
            return false;
        }

        createCursorRequest.Ttl = ttl;

        return true;
    }

    /// <summary>
    ///     Tries to set the <c>Cache</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetCacheParameter(
        this Request<CreateCursorBody> createCursorRequest,
        Dictionary<string, object> parameters)
    {
        return createCursorRequest?.BodyObject != null
            && createCursorRequest.BodyObject.TrySetCacheParameter(parameters);
    }

    /// <summary>
    ///     Tries to set the <c>Cache</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetCacheParameter(
        this CreateCursorBody createCursorRequest,
        Dictionary<string, object> parameters)
    {
        if (createCursorRequest == null
            || parameters == null
            || createCursorRequest.Cache.HasValue
            || !parameters.TryGetValue(ParameterName.Cache, out object raw)
            || !(raw is bool cache))
        {
            return false;
        }

        createCursorRequest.Cache = cache;

        return true;
    }

    /// <summary>
    ///     Tries to set the <c>MemoryLimit</c> parameter, if not already set. if the operation is not successful, it will
    ///     return <c>false</c>.
    /// </summary>
    internal static bool TrySetMemoryLimitParameter(
        this Request<CreateCursorBody> createCursorRequest,
        Dictionary<string, object> parameters)
    {
        return createCursorRequest?.BodyObject != null
            && createCursorRequest.BodyObject.TrySetMemoryLimitParameter(parameters);
    }

    /// <summary>
    ///     Tries to set the <c>MemoryLimit</c> parameter, if not already set. if the operation is not successful, it will
    ///     return <c>false</c>.
    /// </summary>
    internal static bool TrySetMemoryLimitParameter(
        this CreateCursorBody createCursorRequest,
        Dictionary<string, object> parameters)
    {
        if (createCursorRequest == null
            || parameters == null
            || createCursorRequest.MemoryLimit.HasValue
            || !parameters.TryGetValue(ParameterName.MemoryLimit, out object raw)
            || !(raw is long memoryLimit))
        {
            return false;
        }

        createCursorRequest.MemoryLimit = memoryLimit;

        return true;
    }

    /// <summary>
    ///     Tries to set the <c>BatchSize</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetBatchSizeParameter(
        this Request<CreateCursorBody> createCursorRequest,
        Dictionary<string, object> parameters)
    {
        return createCursorRequest?.BodyObject != null
            && createCursorRequest.BodyObject.TrySetBatchSizeParameter(parameters);
    }

    /// <summary>
    ///     Tries to set the <c>BatchSize</c> parameter, if not already set. if the operation is not successful, it will return
    ///     <c>false</c>.
    /// </summary>
    internal static bool TrySetBatchSizeParameter(
        this CreateCursorBody createCursorRequest,
        Dictionary<string, object> parameters)
    {
        if (createCursorRequest == null
            || parameters == null
            || createCursorRequest.BatchSize.HasValue
            || !parameters.TryGetValue(ParameterName.BatchSize, out object raw)
            || !(raw is int batchSize))
        {
            return false;
        }

        createCursorRequest.BatchSize = batchSize;

        return true;
    }

    /// <summary>
    ///     Sets the suggested values of all mandatory properties in <paramref name="createCursorRequest" />, if not already
    ///     set.
    /// </summary>
    internal static Request<CreateCursorBody> NormalizeRequest(this Request<CreateCursorBody> createCursorRequest)
    {
        if (createCursorRequest?.BodyObject == null
            || (createCursorRequest.BodyObject.BatchSize.HasValue
                && createCursorRequest.BodyObject.BatchSize.Value > 0))
        {
            return createCursorRequest;
        }

        createCursorRequest.BodyObject.BatchSize = 1000;

        return createCursorRequest;
    }
}
