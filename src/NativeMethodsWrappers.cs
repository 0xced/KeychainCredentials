using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using static KeychainCredentialsLib.StatusCode;

namespace KeychainCredentialsLib;

/// <summary>
/// Wraps native methods.
/// Takes care of char buffers creation, retrying with a larger buffer if required and
/// transforming non-recoverable error codes into <see cref="KeychainException"/>.
/// </summary>
[SuppressMessage("Performance", "CA1814", Justification = "Jagged arrays can't be marshalled and throw MarshalDirectiveException")]
[SupportedOSPlatform("macOS")]
internal static class NativeMethodsWrappers
{
    public static IReadOnlyCollection<string> GetUserNames(string server, string? authType, int limit, int bufferLength = 128)
    {
        if (server == null) throw new ArgumentNullException(nameof(server));

        var result = new List<string>();
        var maxLength = bufferLength;
        var accounts = new char[limit, maxLength];
        var accountsLength = Enumerable.Repeat(maxLength, limit).ToArray();
        var numberOfAccounts = limit;
        var status = NativeMethods.GetAccounts(server, authType, accounts, accountsLength, ref numberOfAccounts);
        if (status == BufferTooSmall)
        {
            maxLength = accountsLength.Max();
            accounts = new char[limit, maxLength];
            accountsLength = Enumerable.Repeat(maxLength, limit).ToArray();
            status = NativeMethods.GetAccounts(server, authType, accounts, accountsLength, ref numberOfAccounts);
        }

        if (status is SecUserCanceled or SecItemNotFound)
        {
            return result;
        }

        if (status != SecSuccess)
        {
            throw new KeychainException(status);
        }

        for (var i = 0; i < numberOfAccounts; i++)
        {
            var builder = new StringBuilder();
            for (var j = 0; j < accountsLength[i]; j++)
            {
                builder.Append(accounts[i, j]);
            }
            var account = builder.ToString();
            if (account.Length > 0)
            {
                result.Add(account);
            }
        }

        return result;
    }

    public static bool TryGetPassword(string server, string? authType, string userName, [NotNullWhen(true)] out string? password, [NotNullWhen(false)] out UnavailabilityReason? reason, int bufferLength = 128)
    {
        if (server == null) throw new ArgumentNullException(nameof(server));
        if (userName == null) throw new ArgumentNullException(nameof(userName));

        var passwordBuffer = new char[bufferLength];
        var passwordLength = passwordBuffer.Length;
        var status = NativeMethods.GetPassword(server, authType, userName, passwordBuffer, ref passwordLength);
        if (status == BufferTooSmall)
        {
            // The password buffer was not big enough to hold the full password, so we try again with the exact required buffer length
            Array.Resize(ref passwordBuffer, passwordLength);
            status = NativeMethods.GetPassword(server, authType, userName, passwordBuffer, ref passwordLength);
        }

        switch (status)
        {
            case SecUserCanceled:
                password = null;
                reason = UnavailabilityReason.Denied;
                return false;
            case SecItemNotFound:
                password = null;
                reason = UnavailabilityReason.NotFound;
                return false;
            case SecSuccess:
                password = new string(passwordBuffer, 0, passwordLength);
                reason = null;
                return true;
            case BufferTooSmall:
            default:
                throw new KeychainException(status);
        }
    }

    public static string? GetErrorMessage(StatusCode statusCode, int bufferLength = 512)
    {
        var messageBuffer = new char[bufferLength];
        var messageBufferLength = messageBuffer.Length;
        var status = NativeMethods.GetErrorMessage(statusCode, messageBuffer, ref messageBufferLength);
        if (status == BufferTooSmall)
        {
            Array.Resize(ref messageBuffer, messageBufferLength);
            status = NativeMethods.GetErrorMessage(statusCode, messageBuffer, ref messageBufferLength);
        }

        return status == SecSuccess ? new string(messageBuffer, 0, messageBufferLength) : null;
    }
}