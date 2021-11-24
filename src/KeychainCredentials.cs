using static KeychainCredentialsLib.NativeMethodsWrappers;

namespace KeychainCredentialsLib;

/// <summary>
/// Implementation of the <see cref="ICredentials"/> interface using the macOS Keychain.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("macOS")]
public class KeychainCredentials : ICredentials
{
    private readonly IUserNameSelection _userNameSelection;

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    public KeychainCredentials() : this(new DefaultUserNameSelection())
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainCredentials"/> class.
    /// </summary>
    /// <param name="userNameSelection"></param>
    public KeychainCredentials(IUserNameSelection userNameSelection)
    {
        _userNameSelection = userNameSelection ?? throw new ArgumentNullException(nameof(userNameSelection));
    }

    /// <inheritdoc />
    public NetworkCredential? GetCredential(Uri uri, string authType)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));

        var userNames = GetUserNames(uri.Host, authType, _userNameSelection.UserNamesLimit);
        var userName = _userNameSelection.SelectUserName(uri, authType, userNames);
        if (userName is null)
        {
            return null;
        }

        var password = GetPassword(uri.Host, authType, userName);
        if (password is null)
        {
            return null;
        }

        return new NetworkCredential(userName, password);
    }
}