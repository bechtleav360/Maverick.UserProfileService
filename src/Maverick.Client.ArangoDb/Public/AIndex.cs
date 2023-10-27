using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Maverick.Client.ArangoDb.Public.Models.Index;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Provides acces to ArangoDB index API.
/// </summary>
/// <inheritdoc />
public class AIndex : IAIndex
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    internal AIndex(Connection connection)
    {
        _connection = connection;
    }

    private async Task<Response> CreateIndexAsync(
        string collectionName,
        AIndexType indexTyp,
        string[] fields,
        bool? sparse,
        int? minlength,
        bool? geoJson,
        bool? deduplicate,
        int? expireAfter,
        bool? unique)
    {
        var body = new CreateIndexBody
        {
            Type = indexTyp.ToString().ToLower(),
            Fields = fields,
            Sparse = sparse,
            MinLength = minlength,
            GeoJson = geoJson,
            Deduplicate = deduplicate,
            ExpireAfter = expireAfter,
            Unique = unique
        };

        var request = new Request<CreateIndexBody>(HttpMethod.Post, ApiBaseUri.Index, body);
        request.QueryString.Add(ParameterName.Collection, collectionName);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        return response;
    }

    /// <inheritdoc />
    public async Task<GetIndexResponse> GetIndexAsync(string id)
    {
        if (!ADocument.IsId(id))
        {
            throw new ArgumentException("Specified id value (" + id + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Get, ApiBaseUri.Index, "/" + id);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        if (response.IsSuccessStatusCode)
        {
            return new GetIndexResponse(response, response.ParseBody<CollectionIndex>());
        }

        return new GetIndexResponse(response, response.Exception);
    }

    /// <summary>
    ///     Deletes specified index.
    /// </summary>
    /// <exception cref="ArgumentException">Specified id value has invalid format.</exception>
    public async Task<DeleteIndexResponse> DeleteIndexAsync(string id)
    {
        if (!ADocument.IsId(id))
        {
            throw new ArgumentException("Specified id value (" + id + ") has invalid format.");
        }

        var request = new Request(HttpMethod.Delete, ApiBaseUri.Index, "/" + id);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var res = response.ParseBody<Body>();

            return new DeleteIndexResponse(response, res?.Id);
        }

        return new DeleteIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateFullTextIndexResponse> CreateFullTextIndexAsync(
        string collectionName,
        string[] fields,
        int? minLength = null)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Fulltext,
            fields,
            null,
            minLength,
            null,
            null,
            null,
            null);

        if (response.IsSuccessStatusCode)
        {
            return new CreateFullTextIndexResponse(response, response.ParseBody<FullTextIndexResponseEntity>());
        }

        return new CreateFullTextIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateHashIndexResponse> CreateHashIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse = false,
        bool deduplicate = false)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Hash,
            fields,
            sparse,
            null,
            null,
            deduplicate,
            null,
            unique);

        if (response.IsSuccessStatusCode)
        {
            return new CreateHashIndexResponse(response, response.ParseBody<IndexResponseWithSelectivityEntity>());
        }

        return new CreateHashIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreatePersistentIndexResponse> CreatePersistentIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse = false)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Persistent,
            fields,
            sparse,
            null,
            null,
            null,
            null,
            unique);

        if (response.IsSuccessStatusCode)
        {
            return new CreatePersistentIndexResponse(
                response,
                response.ParseBody<IndexResponseWithSelectivityEntity>());
        }

        return new CreatePersistentIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateSkipListIndexResponse> CreateSkipListIndexAsync(
        string collectionName,
        string[] fields,
        bool unique,
        bool sparse = false,
        bool deduplicate = false)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Skiplist,
            fields,
            sparse,
            null,
            null,
            deduplicate,
            null,
            unique);

        if (response.IsSuccessStatusCode)
        {
            return new CreateSkipListIndexResponse(
                response,
                response.ParseBody<IndexResponseWithSelectivityEntity>());
        }

        return new CreateSkipListIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateTtlIndexResponse> CreateTtlIndexAsync(
        string collectionName,
        string[] fields,
        int expireAfter)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Ttl,
            fields,
            null,
            null,
            null,
            null,
            expireAfter,
            null);

        if (response.IsSuccessStatusCode)
        {
            return new CreateTtlIndexResponse(response, response.ParseBody<IndexResponseWithSelectivityEntity>());
        }

        return new CreateTtlIndexResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateGeoIndexResponse> CreateGeoIndexAsync(
        string collectionName,
        string[] fields,
        bool geoJson = false)
    {
        Response response = await CreateIndexAsync(
            collectionName,
            AIndexType.Geo,
            fields,
            null,
            null,
            geoJson,
            null,
            null,
            null);

        if (response.IsSuccessStatusCode)
        {
            return new CreateGeoIndexResponse(response, response.ParseBody<GeoIndexResponseEntity>());
        }

        return new CreateGeoIndexResponse(response, response.Exception);
    }
}
