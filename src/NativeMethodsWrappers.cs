using static KeychainCredentialsLib.StatusCode;

namespace KeychainCredentialsLib;

/// <summary>
/// Wraps native methods.
/// Takes care of char buffers creation, retrying with a larger buffer if required and
/// transforming non-recoverable error codes into <see cref="KeychainException"/>.
/// </summary>
internal static class NativeMethodsWrappers
{
    private const int DefaultBufferLength = 128;

    public static IReadOnlyCollection<string> GetUserNames(string server, string authType, int limit)
    {
        var result = new List<string>();
        long maxLength = DefaultBufferLength;
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

    public static string? GetPassword(string server, string authType, string userName)
    {
        var passwordBuffer = new char[DefaultBufferLength];
        var passwordLength = passwordBuffer.Length;
        var status = NativeMethods.GetPassword(server, authType, userName, passwordBuffer, ref passwordLength);
        if (status == BufferTooSmall)
        {
            // The password buffer was not big enough to hold the full password, so we try again with the exact required buffer length
            Array.Resize(ref passwordBuffer, passwordLength);
            status = NativeMethods.GetPassword(server, authType, userName, passwordBuffer, ref passwordLength);
        }

        if (status is SecUserCanceled or SecItemNotFound)
        {
            return null;
        }

        if (status == SecSuccess)
        {
            return new string(passwordBuffer, 0, passwordLength);
        }

        throw new KeychainException(status);
    }

    public static string? GetErrorMessage(StatusCode statusCode)
    {
        var messageBuffer = new char[512];
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