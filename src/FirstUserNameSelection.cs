namespace KeychainCredentialsLib;

/// <summary>
/// Selects the first username returned by the Keychain for a given URI.
/// </summary>
public class FirstUserNameSelection : IUserNameSelection
{
    private readonly string? _userName;

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    /// <remarks>
    /// Use the constructor that takes a username if you know which username must be used in advance.
    /// </remarks>
    public FirstUserNameSelection()
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    /// <param name="userName">The username to match against the available usernames in the Keychain.</param>
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