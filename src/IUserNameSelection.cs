namespace KeychainCredentialsLib;

/// <summary>
/// Provides a mechanism to choose which user name to use in the event that multiple user names exist for a given URI in the Keychain.
/// </summary>
public interface IUserNameSelection
{
    /// <summary>
    /// The maximum number of user names retrieved from the Keychain for a given URI.
    /// </summary>
    /// <remarks>The size of the <c>userNames</c> parameter in <see cref="SelectUserName"/> can never exceed this limit.</remarks>
    int UserNamesLimit { get; }

    /// <summary>
    /// Select a user name among the possible ones for the given <paramref name="uri"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> for which authentication is required.</param>
    /// <param name="authType">The type of authentication.</param>
    /// <param name="userNames">The collection of available user names in the Keychain to choose from.</param>
    /// <returns>An element from the <paramref name="userNames"/> collection or <see langword="null"/> is none is applicable.</returns>
    string? SelectUserName(Uri uri, string? authType, IReadOnlyCollection<string> userNames);
}