using System.Diagnostics.CodeAnalysis;
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
    public KeychainCredentials() : this(new FirstUserNameSelection())
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
    public NetworkCredential? GetCredential(Uri uri, string? authType)
    {
        return TryGetCredential(uri, authType, out var credential, out _) ? credential : null;
    }

    /// <summary>
    /// Retrieves a <see cref="NetworkCredential"/> object that is associated with the specified URI, and authentication type.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> that the client is providing authentication for.</param>
    /// <param name="authType">The type of authentication, as defined in the <see cref="IAuthenticationModule.AuthenticationType"/> property.</param>
    /// <param name="credential">When this method returns <see langword="true"/>, contains the credential associated with the specified URI and authentication type, if the credential is retrieved from the Keychain; otherwise, <see langword="null"/>.</param>
    /// <param name="reason">When this method returns <see langword="false"/>, contains the reason why the credential was not retrieved from the Keychain; otherwise, <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is null.</exception>
    /// <returns><see langword="true"/> if a credential matching the <paramref name="uri"/> and <paramref name="authType"/> is retrieved from the Keychain; otherwise, <see langword="false"/>.</returns>
    public bool TryGetCredential(Uri uri, string? authType, [NotNullWhen(true)] out NetworkCredential? credential, [NotNullWhen(false)] out UnavailabilityReason? reason)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));

        var userNames = GetUserNames(uri.Host, authType, Math.Max(1, _userNameSelection.UserNamesLimit));
        if (userNames.Count == 0)
        {
            credential = null;
            reason = UnavailabilityReason.NotFound;
            return false;
        }

        var userName = _userNameSelection.SelectUserName(uri, authType, userNames);
        if (userName is null)
        {
            credential = null;
            reason = UnavailabilityReason.NotFound;
            return false;
        }

        if (TryGetPassword(uri.Host, authType, userName, out var password, out reason))
        {
            credential = new NetworkCredential(userName, password);
            return true;
        }

        credential = null;
        return false;
    }
}