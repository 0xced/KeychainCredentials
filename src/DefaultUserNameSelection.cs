namespace KeychainCredentialsLib;

/// <summary>
/// Default implementation of the <see cref="IUserNameSelection"/>.
/// Limits to 10 user names and selects the first proposed user name by the Keychain.
/// </summary>
internal class DefaultUserNameSelection : IUserNameSelection
{
    public string? SelectUserName(Uri uri, string authType, IReadOnlyCollection<string> userNames) => userNames.FirstOrDefault();
}