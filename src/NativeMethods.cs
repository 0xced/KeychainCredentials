namespace KeychainCredentialsLib;

/// <summary>
/// A subset of status codes (OSStatus) returned by the Security framework.
/// The status codes defined in this enum can be handled gracefully, when a status code not
/// defined in this enum is returned, a <see cref="KeychainException"/> is thrown.
/// <para/>
/// See https://developer.apple.com/documentation/security/1542001-security_framework_result_codes for a complete list of the Security framework status codes.
/// </summary>
/// <remarks>
/// Negative codes are actual codes from the Security framework while positive
/// codes are custom status codes used for native interop communication.
/// </remarks>
internal enum StatusCode
{
    SecSuccess = 0,
    SecUserCanceled = -128,
    SecItemNotFound = -25300,
    BufferTooSmall = 25301,
}

internal static class NativeMethods
{
    private const string KeychainCredentialsLib = nameof(KeychainCredentials) + ".Native";

    [DllImport(KeychainCredentialsLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern StatusCode GetAccounts([MarshalAs(UnmanagedType.LPUTF8Str)] string server, [MarshalAs(UnmanagedType.LPUTF8Str)] string authType, char[,] accounts, long[] accountsLength, ref int numberOfAccounts);

    [DllImport(KeychainCredentialsLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern StatusCode GetPassword([MarshalAs(UnmanagedType.LPUTF8Str)] string server, [MarshalAs(UnmanagedType.LPUTF8Str)] string authType, [MarshalAs(UnmanagedType.LPUTF8Str)] string userName, char[] password, ref int passwordLength);

    [DllImport(KeychainCredentialsLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern StatusCode GetErrorMessage(StatusCode statusCode, char[] message, ref int messageLength);
}