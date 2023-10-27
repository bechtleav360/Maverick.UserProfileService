using System;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public class DictatorSettings
{
    /// <summary>
    ///     Global DateTime serialization format. Default is set to Object.
    /// </summary>
    public DateTimeFormat DateTimeFormat { get; set; }

    /// <summary>
    ///     Global DateTime string format which will be used when serializing DateTime object in string format. Default is set
    ///     to "yyyy-MM-ddTHH:mm:ss.fffZ".
    /// </summary>
    public string DateTimeStringFormat { get; set; }

    /// <summary>
    ///     Global enum serialization format. Default is set to Object.
    /// </summary>
    public EnumFormat EnumFormat { get; set; }

    /// <summary>
    ///     Global documents merge behavior. Default is set to OverwriteFields.
    /// </summary>
    public MergeBehavior MergeBehavior { get; set; }

    /// <summary>
    ///     Unix epoch constant.
    /// </summary>
    public DateTime UnixEpoch { get; }

    internal DictatorSettings()
    {
        UnixEpoch = new DateTime(
            1970,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        ResetToDefaults();
    }

    /// <summary>
    ///     Resets global settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        EnumFormat = EnumFormat.Object;
        MergeBehavior = MergeBehavior.OverwriteFields;
        DateTimeFormat = DateTimeFormat.Object;
        DateTimeStringFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    }
}
