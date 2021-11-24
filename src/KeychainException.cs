using static KeychainCredentialsLib.NativeMethodsWrappers;

namespace KeychainCredentialsLib;

#pragma warning disable CA1032

/// <summary>
/// Thrown when the underlying native keychain API returns an error code that can't be handled.
/// </summary>
public class KeychainException : Exception
{
    /// <summary>
    /// Initialize a new instance of the <see cref="KeychainException"/> class.
    /// </summary>
    /// <param name="statusCode">The underlying OSStatus code representing the error.</param>
    internal KeychainException(StatusCode statusCode) : base(GetErrorMessage(statusCode))
    {
        StatusCode = (int)statusCode;
    }

    /// <summary>
    /// The status code returned by the underlying native keychain API
    /// </summary>
    /// <remarks>See https://developer.apple.com/documentation/security/1542001-security_framework_result_codes for a complete list of the Security framework status codes</remarks>
    public int StatusCode { get; }
}