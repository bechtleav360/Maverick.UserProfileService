using System;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains attributes of a specified transaction
/// </summary>
public class TransactionEntity
{
    private string _rawStatus;

    /// <summary>
    ///     Transaction ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Status of the transaction as string value
    /// </summary>
    [JsonProperty("status")]
    public string RawStatus
    {
        get => _rawStatus;
        set => SetStatus(value);
    }

    /// <summary>
    ///     Status of the transaction.
    /// </summary>
    [JsonIgnore]
    public TransactionStatus Status { get; set; }

    private void SetStatus(string value)
    {
        _rawStatus = value;

        Status =
            !string.IsNullOrWhiteSpace(value)
            && Enum.TryParse(value, true, out TransactionStatus status)
                ? status
                : TransactionStatus.Unknown;
    }
}
