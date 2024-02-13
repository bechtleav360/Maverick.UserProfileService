using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UserProfileService.Common.V2.RequestBuilder;

/// <summary>
///     The request builder is needed for building a request, also using
///     optional parameter.
/// </summary>
public class BasicRequestBuilder
{
    /// <summary>
    ///     The fixed relative part of the uri (version &amp; api path)
    /// </summary>
    private readonly string _fixedRelativeUri;

    /// <summary>
    ///     The logger
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///     For storing the optional parameter.
    /// </summary>
    private readonly IDictionary<string, List<string>> _queryParameters = new Dictionary<string, List<string>>();

    /// <summary>
    ///     The request message that is build.
    /// </summary>
    private HttpRequestMessage _requestMessage;

    /// <summary>
    ///     The base url for the service.
    /// </summary>
    private Uri BaseUri { get; }

    /// <summary>
    ///     The optional uri.
    /// </summary>
    private Uri OperationUri { get; set; }

    /// <summary>
    ///     Constructor for the class.
    /// </summary>
    /// <param name="baseUri">The base url for the client.</param>
    /// <param name="loggerFactory">The logger factory for creating a logger.</param>
    public BasicRequestBuilder(
        Uri baseUri,
        ILoggerFactory loggerFactory)
    {
        _requestMessage = new HttpRequestMessage();
        BaseUri = baseUri;
        _fixedRelativeUri = baseUri.PathAndQuery;
        _logger = loggerFactory.CreateLogger(GetType().FullName);
    }

    /// <summary>
    ///     Clean the optional parameter dictionary.
    /// </summary>
    private void Clean()
    {
        _queryParameters.Clear();
        _requestMessage = new HttpRequestMessage();
    }

    /// <summary>
    ///     Build the relative uri for the request.
    /// </summary>
    /// <returns>The relative uri for the request.</returns>
    internal string BuildRelativeUri()
    {
        var uri = new StringBuilder(OperationUri?.AbsoluteUri);

        if (_queryParameters.Count > 0)
        {
            uri.Append("?");

            var index = 0;

            foreach (KeyValuePair<string, List<string>> itemList in _queryParameters)
            {
                foreach (string item in itemList.Value)
                {
                    uri.Append(itemList.Key + "=" + item);

                    index++;

                    if (index != _queryParameters.Count)
                    {
                        uri.Append("&");
                    }
                }
            }
        }

        _logger.LogInformation("Generated request uri.", uri.ToString());

        return uri.ToString();
    }

    /// <summary>
    ///     Building a request for the client.
    /// </summary>
    /// <returns>Returns a <see cref="HttpRequestMessage" /> object.</returns>
    public HttpRequestMessage BuildRequest()
    {
        _requestMessage.RequestUri = new Uri(BuildRelativeUri());
        HttpRequestMessage result = _requestMessage;
        Clean();

        return result;
    }

    /// <summary>
    ///     Sets the method for the request.
    /// </summary>
    /// <param name="httpMethod">The method type for the request.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder SetMethod(HttpMethod httpMethod)
    {
        if (httpMethod == null)
        {
            throw new ArgumentNullException(nameof(httpMethod));
        }

        _requestMessage.Method = httpMethod;

        return this;
    }

    /// <summary>
    ///     Set the content, if is needed.
    /// </summary>
    /// <param name="content">The content that has to be set.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder SetContent(HttpContent content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        _requestMessage.Content = content;

        return this;
    }

    /// <summary>
    ///     Add Header parameter and its value into the <see cref="HttpHeaders" /> of the <see cref="HttpRequestMessage" />
    /// </summary>
    /// <param name="headerParameterName">Set name of the header parameter.</param>
    /// <param name="value">The value for the header.</param>
    /// <param name="throwException">
    ///     If set to <see langword="false"/>,
    ///     the argument exceptions are suppressed. <see langword="true"/> by default.
    /// </param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddHeaders(string headerParameterName, string value, bool throwException = true)
    {
        if (headerParameterName == null && throwException)
        {
            throw new ArgumentNullException(nameof(headerParameterName));
        }

        if (value == null && throwException)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (_requestMessage?.Headers == null)
        {
            _logger.LogWarning(
                "Unable to add Header in RequestBuilder (Headers is null). Request message: '{requestMessage}'",
                _requestMessage);

            return this;
        }

        if (headerParameterName != null && value != null)
        {
            _requestMessage.Headers.Add(headerParameterName, value);
        }

        return this;
    }

    /// <summary>
    ///     Set the operation uri that contains the baseUri and the request url.
    /// </summary>
    /// <param name="requestUrl">The request url.</param>
    /// <param name="rootPath">The root path.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder SetUri(string requestUrl, string rootPath = "api")
    {
        if (requestUrl == null)
        {
            throw new ArgumentNullException(nameof(requestUrl));
        }

        OperationUri = new Uri(
            BaseUri,
            _fixedRelativeUri
            + rootPath
            + "/"
            + requestUrl.TrimStart('/'));

        return this;
    }

    /// <summary>
    ///     Add a (string) query parameter to the request
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddQueryParameter(string parameterName, string value)
    {
        if (value == null)
        {
            _logger.LogWarning(string.Format("Tried adding null value as \"{0}\" parameter", parameterName));

            return this;
        }

        _queryParameters.Add(
            parameterName,
            new List<string>
            {
                value
            });

        return this;
    }

    /// <summary>
    ///     Add an <see cref="Enum" /> query parameter to the request.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <param name="condition">Condition when the query parameter should be added.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddQueryParameter<TEnum>(string parameterName, TEnum value, bool condition = true)
        where TEnum : Enum
    {
        if (!condition)
        {
            _logger.LogTrace("Condition not reached. Skipping setting query parameter.");

            return this;
        }

        if (value == null)
        {
            _logger.LogWarning("Tried adding null value as {pName} parameter", parameterName);

            return this;
        }

        _queryParameters.Add(
            parameterName,
            new List<string>
            {
                value.ToString("G")
            });

        return this;
    }

    /// <summary>
    ///     Add a (int) query parameter to the request
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddQueryParameter(string parameterName, int value)
    {
        return AddQueryParameter(parameterName, value.ToString());
    }

    /// <summary>
    ///     Add a (bool) query parameter to the request
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value of the parameter, in this case a bool.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddQueryParameter(string parameterName, bool value)
    {
        return AddQueryParameter(parameterName, value.ToString());
    }

    /// <summary>
    ///     Add a collection of strings as parameter to the request
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="values">The values of the parameter, in this case a collection of strings.</param>
    /// <returns>Returns the <see cref="RequestBuilder" /> object.</returns>
    public BasicRequestBuilder AddQueryParameter(string parameterName, IEnumerable<string> values)
    {
        _queryParameters.Add(parameterName, values.ToList());

        return this;
    }

    /// <summary>
    ///     Sets the body of the request, that can be generic type.
    /// </summary>
    /// <typeparam name="T">The generic type that has to be serialized.</typeparam>
    /// <param name="bodyObject">The object, that has to be serialized.</param>
    /// <param name="mediaType">The media type of the object stored in the body.</param>
    /// <returns></returns>
    public BasicRequestBuilder SetBody<T>(T bodyObject, string mediaType = "application/json")
    {
        _requestMessage.Content =
            new StringContent(JsonConvert.SerializeObject(bodyObject), Encoding.UTF8, mediaType);

        _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

        return this;
    }
}
