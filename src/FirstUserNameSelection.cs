namespace KeychainCredentialsLib;

/// <summary>
/// Selects the first user name returned by the Keychain for a given URI.
/// </summary>
public class FirstUserNameSelection : IUserNameSelection
{
    private readonly string? _userName;

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    /// <remarks>
    /// Use the constructor that takes a user name if you know which user name must be used in advance.
    /// </remarks>
    public FirstUserNameSelection()
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    /// <param name="userName">The user name to match against the available user names in the Keychain.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="userName"/> is null.</exception>
    public FirstUserNameSelection(string userName)
    {
        _userName = userName ?? throw new ArgumentNullException(nameof(userName));
    }

    /// <inheritdoc />
    public int UserNamesLimit => _userName == null ? 1 : 16;

    /// <inheritdoc />
    public string? SelectUserName(Uri uri, string? authType, IReadOnlyCollection<string> userNames)
    {
        return _userName != null ? userNames.FirstOrDefault(e => e == _userName) : userNames.FirstOrDefault();
    }
}