namespace AIos.Core.Infrastructure;

/// <summary>Bridges <see cref="AIos.Sdk.Chat.ChatRole"/> to <see cref="Microsoft.Extensions.AI.ChatRole"/> for use with Microsoft.Extensions.AI APIs.</summary>
public static class ChatRoleExtensions
{
    /// <summary>Converts this SDK role to <see cref="Microsoft.Extensions.AI.ChatRole"/>.</summary>
    public static Microsoft.Extensions.AI.ChatRole ToExtensionsAiRole(this Sdk.Chat.ChatRole role) => new(role.Value);
}
