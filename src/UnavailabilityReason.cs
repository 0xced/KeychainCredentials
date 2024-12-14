namespace KeychainCredentialsLib;

/// <summary>
/// Possible reasons why credentials were not retrieved from the Keychain.
/// </summary>
public enum UnavailabilityReason
{
    /// <summary>
    /// The requested credentials do not exist in the Keychain.
    /// </summary>
    NotFound,

    /// <summary>
    /// The user clicked the <c>Deny</c> button in the Keychain dialog, preventing the credentials to be retrieved.
    /// </summary>
    Denied,
}