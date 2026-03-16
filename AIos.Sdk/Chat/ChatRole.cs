using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AIos.Sdk.Chat;

[DebuggerDisplay("{Value,nq}")]
public record ChatRole
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatRole" /> struct with the provided value.
    /// </summary>
    /// <param name="value">The value to associate with this <see cref="ChatRole" />.</param>
    [JsonConstructor]
    public ChatRole(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? throw new InvalidEnumArgumentException(value) : value;
    }

    /// <summary>Gets the role that instructs or sets the behavior of the system.</summary>
    public static ChatRole System { get; } = new("system");

    /// <summary>Gets the role that provides responses to system-instructed, user-prompted input.</summary>
    public static ChatRole Assistant { get; } = new("assistant");

    /// <summary>Gets the role that provides user input for chat interactions.</summary>
    public static ChatRole User { get; } = new("user");

    /// <summary>Gets the role that provides additional information and references in response to tool use requests.</summary>
    public static ChatRole Tool { get; } = new("tool");

    /// <summary>
    ///     Gets the value associated with this <see cref="ChatRole" />.
    /// </summary>
    /// <remarks>
    ///     The value will be serialized into the "role" message field of the Chat Message format.
    /// </remarks>
    public string Value { get; }
}