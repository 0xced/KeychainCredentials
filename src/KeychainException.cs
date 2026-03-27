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
    /// <remarks>
    /// See <see href="https://github.com/apple-oss-distributions/Security/blob/db15acbe6a7f257a859ad9a3bb86097bfe0679d9/base/SecBase.h#L324-L716">SecBase.h</see> for a complete list of the Security framework status codes.
    /// There is also the <see href="https://developer.apple.com/documentation/security/security-framework-result-codes">Security Framework Result Codes</see>, but it doesn't include the actual status code values.
    /// </remarks>
    public int StatusCode { get; }
}