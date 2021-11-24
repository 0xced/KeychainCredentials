namespace KeychainCredentialsLib;

/// <summary>
/// Provides a mechanism to choose which user name to use in the event
/// that multiple user names exist for a single URI in the Keychain.
/// </summary>
public interface IUserNameSelection
{
    /// <summary>
    /// The maximum number of user names retrieved from the Keychain for a given URI.
    /// <para/>
    /// Defaults to 10.
    /// </summary>
    /// <remarks>The size of the <c>userNames</c> parameter in <see cref="SelectUserName"/> can never exceed this limit.</remarks>
    int UserNamesLimit => 10;

    /// <summary>
    ///
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="authType"></param>
    /// <param name="userNames"></param>
    /// <returns></returns>
    string? SelectUserName(Uri uri, string authType, IReadOnlyCollection<string> userNames);
}