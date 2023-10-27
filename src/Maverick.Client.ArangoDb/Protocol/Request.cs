using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Maverick.Client.ArangoDb.ExternalLibraries.dictator;
using Newtonsoft.Json;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

internal class Request
{
    internal WebHeaderCollection Headers = new WebHeaderCollection();
    internal Dictionary<string, string> QueryString = new Dictionary<string, string>();
    internal string BodyAsString { get; set; }
    internal HttpMethod HttpMethod { get; set; }
    internal string OperationUri { get; set; }

    /// <summary>
    ///     Can contain information about a transaction to help to debug (i.e. read and write collections).
    /// </summary>
    internal string TransactionInformation { get; set; }

    internal Request(HttpMethod httpMethod, string apiUri, string operationUri = "")
    {
        HttpMethod = httpMethod;
        OperationUri = apiUri + operationUri;
    }

    internal string GetRelativeUri()
    {
        var uri = new StringBuilder(OperationUri);

        if (QueryString.Count > 0)
        {
            uri.Append("?");

            var index = 0;

            foreach (KeyValuePair<string, string> item in QueryString)
            {
                uri.Append(item.Key + "=" + item.Value);

                index++;

                if (index != QueryString.Count)
                {
                    uri.Append("&");
                }
            }
        }

        return uri.ToString();
    }

    /// <summary>
    ///     Tries to set the transaction id as header value.
    ///     It will first try to use the <paramref name="transactionId" /> parameter.
    ///     If this is null or empty, it will use the parameters property bag to set a transaction id.
    ///     If none has appropriate values it will return <c>false</c>.
    /// </summary>
    internal bool TrySetTransactionId(
        Dictionary<string, object> parameters,
        string transactionId = null)
    {
        return TrySetHeaderParameter(ParameterName.TransactionId, parameters, transactionId);
    }

    internal bool TryActivateDirtyRead(Dictionary<string, object> parameters)
    {
        return TrySetHeaderParameter(ParameterName.HeaderforceDirtyRead, parameters, bool.TrueString.ToLower());
    }

    /// <summary>
    ///     Tries to set a header value with key <paramref name="parameterName" />. It will use the optional
    ///     <paramref name="explicitValue" /> parameter. If is is null or empty, the <paramref name="parameters" />
    ///     property bag will be used to set the header (this represents the default way to store properties).
    ///     If none has a value to be used, the method will return <c>false</c>.
    /// </summary>
    internal bool TrySetHeaderParameter(
        string parameterName,
        Dictionary<string, object> parameters,
        string explicitValue = null)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name should be provided.", nameof(parameterName));
        }

        if (!string.IsNullOrWhiteSpace(explicitValue))
        {
            Headers.Add(parameterName, explicitValue);

            return true;
        }

        if (parameters != null
            && parameters.ContainsKey(parameterName)
            && parameters[parameterName] != null)
        {
            Headers.Add(parameterName, parameters.String(parameterName));

            return true;
        }

        return false;
    }

    internal void TrySetQueryStringParameter(string parameterName, Dictionary<string, object> parameters)
    {
        if (parameters.ContainsKey(parameterName))
        {
            QueryString.Add(parameterName, parameters.String(parameterName));
        }
    }

    internal static void TrySetBodyParameter(
        string parameterName,
        Dictionary<string, object> source,
        Dictionary<string, object> destination)
    {
        if (source.Has(parameterName))
        {
            destination.Object(parameterName, source.Object(parameterName));
        }
    }
}

internal class Request<T> : Request
{
    internal T BodyObject { get; set; }

    internal Request(HttpMethod httpMethod, string apiUri, T bodyObject, string operationUri = "") : base(
        httpMethod,
        apiUri,
        operationUri)
    {
        BodyObject = bodyObject;
    }

    internal void SerializeBody(JsonSerializerSettings settings = null)
    {
        if (settings == null)
        {
            settings = Serializer.GetDefaultJsonSettings();
        }

        if (BodyObject == null)
        {
            throw new ArgumentNullException(nameof(BodyObject), "Body object should not be null");
        }

        BodyAsString = BodyObject.SerializeObject(settings);
    }
}
