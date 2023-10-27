using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Models.Collection;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET
// with some bug fixes and extensions to support asynchronous operations
// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     A class for interacting with ArangoDB Collections endpoints.
/// </summary>
/// <inheritdoc />
public class ACollection : IACollection
{
    private readonly Connection _connection;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    internal ACollection(Connection connection)
    {
        _connection = connection;
    }

    private async Task<SingleApiResponse<TResponse>> GetInternalAsync<TResponse>(
        string apibaseuri,
        string subUriString,
        IEnumerable<KeyValuePair<string, string>> queryStringParameter = null,
        bool clearParameters = true,
        bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, apibaseuri, $"/{subUriString}");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        KeyValuePair<string, string>[] parameters = queryStringParameter?.Distinct().ToArray();

        if (parameters != null && parameters.Any())
        {
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                request.QueryString.Add(parameter.Key, parameter.Value);
            }
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);

        if (response.IsSuccessStatusCode)
        {
            if (clearParameters)
            {
                _parameters.Clear();
            }

            TResponse body;

            if (response.ResponseBodyAsString == null)
            {
                body = default;
            }
            else if (typeof(TResponse) == typeof(string))
            {
                body = (TResponse)(object)response.ResponseBodyAsString;
            }
            else
            {
                body = response.ParseBody<TResponse>();
            }

            return new SingleApiResponse<TResponse>(response, body);
        }

        _parameters.Clear();

        return new SingleApiResponse<TResponse>(
            response,
            new ApiErrorException(response.ParseBody<ArangoErrorResponse>()));
    }

    /// <inheritdoc />
    public async Task<GetCheckSumResponse> GetChecksumAsync(
        string collectionName,
        bool withRevisions = true,
        bool withData = false,
        bool forceDirtyRead = false)
    {
        var request = new Request(
            HttpMethod.Get,
            ApiBaseUri.Collection,
            $"/{collectionName}/checksum?withRevisions={withRevisions}&withData={withData}");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (!response.IsSuccessStatusCode)
        {
            return new GetCheckSumResponse(response, response.Exception);
        }

        var collection = response.ParseBody<CollectionWithCheckSumAndRevisionIdEntity>();

        return new GetCheckSumResponse(response, collection);
    }

    /// <inheritdoc />
    public async Task<GetAllIndexesResponse> GetAllCollectionIndexesAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Index, $"?collection={collectionName}");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (!response.IsSuccessStatusCode)
        {
            return new GetAllIndexesResponse(response, response.Exception);
        }

        var result = response.ParseBody<GetAllIndexesEntity>();

        return new GetAllIndexesResponse(response, result);
    }

    /// <inheritdoc />
    public async Task<TruncateCollectionResponse> TruncateCollectionAsync(
        string collectionName,
        string transactionId = null)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collectionName));
        }

        var request = new Request(HttpMethod.Put, ApiBaseUri.Collection, "/" + collectionName + "/truncate");
        request.TrySetTransactionId(_parameters, transactionId);

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var collection = response.ParseBody<CollectionEntity>();

            return new TruncateCollectionResponse(response, collection);
        }

        return new TruncateCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<LoadCollectionResponse> LoadCollectionAsync(string collectionName, bool count = true)
    {
        Count(count);
        var bodyDocument = new Dictionary<string, object>();

        // optional
        Request.TrySetBodyParameter(ParameterName.Count, _parameters, bodyDocument);

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Put,
            ApiBaseUri.Collection,
            bodyDocument,
            "/" + collectionName + "/load");

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var collection = response.ParseBody<LoadCollectionEntity>();

            return new LoadCollectionResponse(response, collection);
        }

        return new LoadCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<UnloadCollectionResponse> UnloadCollectionAsync(string collectionName)
    {
        var request = new Request(HttpMethod.Put, ApiBaseUri.Collection, "/" + collectionName + "/unload");

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var collection = response.ParseBody<CollectionEntity>();

            return new UnloadCollectionResponse(response, collection);
        }

        return new UnloadCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<RenameCollectionResponse> RenameCollectionAsync(
        string collectionName,
        string newCollectionName)
    {
        Dictionary<string, object> bodyDocument = new Dictionary<string, object>()
            .String(ParameterName.Name, newCollectionName);

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Put,
            ApiBaseUri.Collection,
            bodyDocument,
            "/" + collectionName + "/rename");

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var collection = response.ParseBody<CollectionEntity>();

            return new RenameCollectionResponse(response, collection);
        }

        return new RenameCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<RotateJournalResponse> RotateCollectionJournalAsync(string collectionName)
    {
        var request = new Request(HttpMethod.Put, ApiBaseUri.Collection, "/" + collectionName + "/rotate");

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var resultObject = response.ParseBody<Dictionary<string, object>>();
            var result = false;

            try
            {
                result = Convert.ToBoolean(resultObject["result"].ToString());
            }
            catch (FormatException ex)
            {
                response.Exception ??= new Exception("Error while parsing result field ", ex);
            }

            return new RotateJournalResponse(response, result);
        }

        return new RotateJournalResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<DeleteCollectionResponse> DeleteCollectionAsync(string collectionName)
    {
        var request = new Request(HttpMethod.Delete, ApiBaseUri.Collection, "/" + $"{collectionName}");
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var deletedCollectionIdObject = response.ParseBody<Dictionary<string, object>>();
            string deletedCollectionId = null;

            try
            {
                deletedCollectionId = deletedCollectionIdObject["id"].ToString();
            }
            catch (Exception ex)
            {
                response.Exception ??= new Exception(
                    "Error while extracting the collection id from original result",
                    ex);
            }

            return new DeleteCollectionResponse(response, deletedCollectionId);
        }

        return new DeleteCollectionResponse(response, response.Exception);
    }

    /// <summary>
    ///     Determines type of the collection. Default value: Document.
    /// </summary>
    /// <inheritdoc />
    public ACollection Type(ACollectionType value)
    {
        // set enum format explicitely to override global setting
        _parameters.Enum(ParameterName.Type, value, EnumFormat.Integer);

        return this;
    }

    /// <summary>
    ///     Determines whether or not to wait until data are synchronised to disk. Default value: false.
    /// </summary>
    /// <inheritdoc />
    public ACollection WaitForSync(bool value)
    {
        _parameters.Bool(ParameterName.WaitForSync, value);

        return this;
    }

    /// <summary>
    ///     Determines maximum size of a journal or datafile in bytes. Default value: server configured.
    /// </summary>
    /// <inheritdoc />
    public ACollection JournalSize(long value)
    {
        _parameters.Long(ParameterName.JournalSize, value);

        return this;
    }

    /// <summary>
    ///     Determines whether the collection will be compacted. Default value: true.
    /// </summary>
    /// <inheritdoc />
    public ACollection DoCompact(bool value)
    {
        _parameters.Bool(ParameterName.DoCompact, value);

        return this;
    }

    /// <summary>
    ///     Determines whether the collection is a system collection. Default value: false.
    /// </summary>
    /// <inheritdoc />
    public ACollection IsSystem(bool value)
    {
        _parameters.Bool(ParameterName.IsSystem, value);

        return this;
    }

    /// <summary>
    ///     Determines whether the collection data is kept in-memory only and not made persistent. Default value: false.
    /// </summary>
    /// <inheritdoc />
    public ACollection IsVolatile(bool value)
    {
        _parameters.Bool(ParameterName.IsVolatile, value);

        return this;
    }

    /// <summary>
    ///     Determines the type of the key generator. Default value: Traditional.
    /// </summary>
    public ACollection KeyGeneratorType(AKeyGeneratorType value)
    {
        // needs to be in string format - set enum format explicitely to override global setting
        _parameters.Enum(ParameterName.KeyOptionsType, value.ToString().ToLower(), EnumFormat.String);

        return this;
    }

    /// <summary>
    ///     Determines whether it is allowed to supply custom key values in the _key attribute of a document. Default value:
    ///     true.
    /// </summary>
    public ACollection AllowUserKeys(bool value)
    {
        _parameters.Bool(ParameterName.KeyOptionsAllowUserKeys, value);

        return this;
    }

    /// <summary>
    ///     Determines increment value for autoincrement key generator.
    /// </summary>
    public ACollection KeyIncrement(long value)
    {
        _parameters.Long(ParameterName.KeyOptionsIncrement, value);

        return this;
    }

    /// <summary>
    ///     Determines initial offset value for autoincrement key generator.
    /// </summary>
    public ACollection KeyOffset(long value)
    {
        _parameters.Long(ParameterName.KeyOptionsOffset, value);

        return this;
    }

    /// <summary>
    ///     Determines the number of shards to create for the collection in cluster environment. Default value: 1.
    /// </summary>
    /// <inheritdoc />
    public ACollection NumberOfShards(int value)
    {
        _parameters.Int(ParameterName.NumberOfShards, value);

        return this;
    }

    /// <summary>
    ///     Determines which document attributes are used to specify the target shard for documents in cluster environment.
    ///     Default value: ["_key"].
    /// </summary>
    /// <inheritdoc />
    public ACollection ShardKeys(List<string> value)
    {
        _parameters.List(ParameterName.ShardKeys, value);

        return this;
    }

    /// <summary>
    ///     This attribute specifies the name of the sharding strategy to use for
    ///     the collection
    /// </summary>
    /// <param name="shardingStrategy"></param>
    /// <inheritdoc />
    public ACollection ShardingStrategy(string shardingStrategy)
    {
        _parameters.String(ParameterName.ShardingStrategy, shardingStrategy);

        return this;
    }

    /// <summary>
    ///     In an Enterprise Edition cluster, this attribute determines an attribute
    ///     of the collection that must contain the shard key value of the referred-to
    ///     smart join collection
    /// </summary>
    /// <param name="smartJoinAttribute"></param>
    /// <inheritdoc />
    public ACollection SmartJoinAttribute(string smartJoinAttribute)
    {
        _parameters.String(ParameterName.SmartJoinAttribute, smartJoinAttribute);

        return this;
    }

    /// <summary>
    ///     he number of buckets into which indexes using a hash
    ///     table are split
    /// </summary>
    /// <param name="indexBuckets"></param>
    /// <inheritdoc />
    public ACollection IndexBuckets(int indexBuckets)
    {
        _parameters.Int(ParameterName.IndexBuckets, indexBuckets);

        return this;
    }

    /// <summary>
    ///     (The default is ""): in an Enterprise Edition cluster, this attribute binds
    ///     the specifics of sharding for the newly created collection to follow that of a
    ///     specified existing collection.
    /// </summary>
    /// <param name="distributeShardsLike"></param>
    /// <inheritdoc />
    public ACollection DistributeShardsLike(string distributeShardsLike)
    {
        _parameters.String(ParameterName.DistributeShardsLike, distributeShardsLike);

        return this;
    }

    /// <summary>
    ///     (The default is 1): in a cluster, this attribute determines how many copies
    ///     of each shard are kept on different DBServers
    /// </summary>
    /// <param name="replicationFactor"></param>
    /// <inheritdoc />
    public ACollection ReplicationFactor(int replicationFactor)
    {
        _parameters.Int(ParameterName.ReplicationFactor, replicationFactor);

        return this;
    }

    /// <summary>
    ///     Determines whether the return value should include the number of documents in collection. Default value: true.
    /// </summary>
    /// <inheritdoc />
    public ACollection Count(bool value)
    {
        _parameters.Bool(ParameterName.Count, value);

        return this;
    }

    /// <summary>
    ///     Determines whether to include document revision ids in the checksum calculation. Default value: false.
    /// </summary>
    public ACollection WithRevisions(bool value)
    {
        // needs to be in string format
        _parameters.String(ParameterName.WithRevisions, value.ToString().ToLower());

        return this;
    }

    /// <summary>
    ///     Determines whether to include document body data in the checksum calculation. Default value: false.
    /// </summary>
    public ACollection WithData(bool value)
    {
        // needs to be in string format
        _parameters.String(ParameterName.WithData, value.ToString().ToLower());

        return this;
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateAsync(string collectionName)
    {
        var bodyDocument = new Dictionary<string, object>();

        // required
        bodyDocument.String(ParameterName.Name, collectionName);
        // optional
        Request.TrySetBodyParameter(ParameterName.Type, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.WaitForSync, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.JournalSize, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.DoCompact, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.IsSystem, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.IsVolatile, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.KeyOptionsType, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.KeyOptionsAllowUserKeys, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.KeyOptionsIncrement, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.KeyOptionsOffset, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.NumberOfShards, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.ShardKeys, _parameters, bodyDocument);

        // optional
        Request.TrySetBodyParameter(ParameterName.ShardingStrategy, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.SmartJoinAttribute, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.IndexBuckets, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.DistributeShardsLike, _parameters, bodyDocument);
        // optional
        Request.TrySetBodyParameter(ParameterName.ReplicationFactor, _parameters, bodyDocument);

        var request = new Request<Dictionary<string, object>>(HttpMethod.Post, ApiBaseUri.Collection, bodyDocument);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new CreateCollectionResponse(response, response.ParseBody<CreateCollectionResponseEntity>());
        }

        return new CreateCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateCollectionAsync(CreateCollectionBody collectionBody)
    {
        var request = new Request<CreateCollectionBody>(HttpMethod.Post, ApiBaseUri.Collection, collectionBody);
        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new CreateCollectionResponse(response, response.ParseBody<CreateCollectionResponseEntity>());
        }

        return new CreateCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<CreateCollectionResponse> CreateCollectionAsync(
        string collectionName,
        ACollectionType collectionType = ACollectionType.Document,
        CreateCollectionOptions collectionOptions = null)
    {
        CreateCollectionBody collectionBody = collectionOptions == null
            ? new CreateCollectionBody
            {
                Name = collectionName,
                Type = collectionType
            }
            : new CreateCollectionBody
            {
                Name = collectionName,
                Type = collectionType,
                DistributeShardsLike = collectionOptions.DistributeShardsLike,
                DoCompact = collectionOptions.DoCompact,
                IndexBuckets = collectionOptions.IndexBuckets,
                IsSystem = collectionOptions.IsSystem,
                IsVolatile = collectionOptions.IsVolatile,
                JournalSize = collectionOptions.JournalSize,
                KeyOptions = collectionOptions.KeyOptions,
                NumberOfShards = collectionOptions.NumberOfShards,
                ReplicationFactor = collectionOptions.ReplicationFactor,
                ShardingStrategy = collectionOptions.ShardingStrategy,
                ShardKeys = collectionOptions.ShardKeys,
                SmartJoinAttribute = collectionOptions.SmartJoinAttribute,
                WaitForSync = collectionOptions.WaitForSync,
                WriteConcern = collectionOptions.WriteConcern
            };

        return await CreateCollectionAsync(collectionBody);
    }

    /// <inheritdoc />
    public async Task<GetCollectionResponse> GetCollectionAsync(string collectionName, bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request, forceDirtyRead);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new GetCollectionResponse(response, response.ParseBody<CollectionEntity>());
        }

        return new GetCollectionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await GetInternalAsync<TResponse>(ApiBaseUri.Collection, collectionName).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionPropertiesAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await GetInternalAsync<TResponse>(
            ApiBaseUri.Collection,
            $"{collectionName}/properties",
            forceDirtyRead: forceDirtyRead);
    }

    /// <inheritdoc />
    public async Task<GetCollectionPropertiesResponse> GetCollectionPropertiesAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}/properties");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        return response.IsSuccessStatusCode
            ? new GetCollectionPropertiesResponse(response, response.ParseBody<CollectionWithDetailEntity>())
            : new GetCollectionPropertiesResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetCollectionCountResponse> GetCollectionCountAsync(
        string collectionName,
        string transactionId = null,
        bool forceDirtyRead = false)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(collectionName));
        }

        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}/count");
        request.TrySetTransactionId(_parameters, transactionId);

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            return new GetCollectionCountResponse(response, response.ParseBody<CollectionCountEntity>());
        }

        return new GetCollectionCountResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<CollectionCountEntity>> GetCollectionCountAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead = false)
    {
        return await GetInternalAsync<CollectionCountEntity>(
                ApiBaseUri.Collection,
                $"{collectionName}/count",
                forceDirtyRead: forceDirtyRead)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SingleApiResponse<TResponse>> GetCollectionFiguresAsync<TResponse>(
        string collectionName,
        bool forceDirtyRead)
    {
        return await GetInternalAsync<TResponse>(ApiBaseUri.Collection, $"{collectionName}/figures")
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GetFiguresResponse> GetCollectionFiguresAsync(
        string collectionName,
        bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}/figures");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        return response.IsSuccessStatusCode
            ? new GetFiguresResponse(response, response.ParseBody<CollectionFiguresEntity>())
            : new GetFiguresResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<GetRevisionIdResponse> GetRevisionIdAsync(string collectionName, bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}/revision");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (!response.IsSuccessStatusCode)
        {
            return new GetRevisionIdResponse(response, response.Exception);
        }

        string revisionId = response.ParseBody<CollectionWithRevisionEntity>()?.Revision;

        return new GetRevisionIdResponse(response, revisionId);
    }

    /// <inheritdoc />
    public async Task<GetRevisionResponse> GetRevisionAsync(string collectionName, bool forceDirtyRead = false)
    {
        var request = new Request(HttpMethod.Get, ApiBaseUri.Collection, $"/{collectionName}/revision");

        if (forceDirtyRead)
        {
            request.TryActivateDirtyRead(_parameters);
        }

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        return response.IsSuccessStatusCode
            ? new GetRevisionResponse(response, response.ParseBody<CollectionWithRevisionEntity>())
            : new GetRevisionResponse(response, response.Exception);
    }

    /// <inheritdoc />
    public async Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(
        CollectionPropertyEntity collectionProperty)
    {
        return await ChangeCollectionPropertiesAsync(
            collectionProperty?.Name,
            collectionProperty?.WaitForSync,
            collectionProperty?.JournalSize);
    }

    /// <inheritdoc />
    public async Task<ChangePropertiesResponse> ChangeCollectionPropertiesAsync(
        string collectionName,
        bool? waitForSync = null,
        long? journalSize = null)
    {
        var bodyDocument = new Dictionary<string, object>();

        if (waitForSync != null)
        {
            WaitForSync(waitForSync.Value);
        }

        Request.TrySetBodyParameter(ParameterName.WaitForSync, _parameters, bodyDocument);

        if (journalSize != null)
        {
            JournalSize(journalSize.Value);
        }

        Request.TrySetBodyParameter(ParameterName.JournalSize, _parameters, bodyDocument);

        var request = new Request<Dictionary<string, object>>(
            HttpMethod.Put,
            ApiBaseUri.Collection,
            bodyDocument,
            "/" + collectionName + "/properties");

        Response response = await RequestHandler.ExecuteAsync(_connection, request);
        _parameters.Clear();

        if (response.IsSuccessStatusCode)
        {
            var collection = response.ParseBody<CollectionWithDetailEntity>();

            return new ChangePropertiesResponse(response, collection);
        }

        return new ChangePropertiesResponse(response, response.Exception);
    }
}
