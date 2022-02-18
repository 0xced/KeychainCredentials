using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace KeychainCredentialsLib.Tests;

[CollectionDefinition("Keychain collection")]
public class KeychainCollection : ICollectionFixture<KeychainFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [KeychainCollection] and all the
    // ICollectionFixture<> interfaces.
    // See https://xunit.net/docs/shared-context#collection-fixture
}

public class KeychainFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    private readonly string _keychainName;
    private readonly string _keychainPassword;

    public KeychainFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));

        var guid = Guid.NewGuid();
        _keychainName = $"KeychainCredentialsLib.Tests-{guid}.keychain";
        _keychainPassword = "1234";
    }

    public async Task AddInternetPasswordAsync(Uri uri, string userName, string password)
    {
        var protocol = uri.Scheme switch
        {
            "ftp" => "ftp ", // https://developer.apple.com/documentation/security/secprotocoltype/ksecprotocoltypeftp
            "ftps" => "ftps", // https://developer.apple.com/documentation/security/secprotocoltype/ksecprotocoltypeftps
            "http" => "http", // https://developer.apple.com/documentation/security/secprotocoltype/ksecprotocoltypehttp
            "https" => "htps", // https://developer.apple.com/documentation/security/secprotocoltype/ksecprotocoltypehttps
            _ => throw new NotImplementedException($"Scheme {uri.Scheme} to SecProtocolType is not implemented")
        };

        await Cli.Wrap("security")
            .WithArguments(new[] {
                "add-internet-password",
                "-r", protocol,
                "-s", uri.Host,
                "-a", userName,
                "-w", password,
                _keychainName
            })
            .ExecuteAsync();

        // The password becomes available only after the new keychain is opened in the Keychain Access app ¯\_(ツ)_/¯
        var keychainPath = Environment.ExpandEnvironmentVariables($"%HOME%/Library/Keychains/{_keychainName}-db");
        await Cli.Wrap("open")
            .WithArguments(new[]
            {
                "-g", // do not bring the application to the foreground
                keychainPath
            }).ExecuteAsync();

        // Wait for the password to be available (max 10s) by polling with `security find-internet-password`
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationTokenSource.Token);

            // We search for the host and DO NOT PASS the _keychainName because that's how SecItemCopyMatching seems to work
            // (you can't specify a keychain, using kSecUseKeychain apparently has no effect)
            // This ensures that the keychain fixture is properly setup and that testing the KeychainCredentials class will work
            var result = await Cli.Wrap("security")
                .WithValidation(CommandResultValidation.None)
                .WithArguments(new[] {
                    "find-internet-password",
                    "-s", uri.Host,
                })
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line => _messageSink.OnMessage(new DiagnosticMessage($"*** find-internet-password *** {line}"))))
                .ExecuteAsync();
            if (result.ExitCode == 0)
            {
                break;
            }
        }
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await Cli.Wrap("security")
            .WithArguments(new[] {
                "create-keychain",
                "-p", _keychainPassword,
                _keychainName
            })
            .ExecuteAsync();

        await AddInternetPasswordAsync(new Uri("https://www.keychain-credentials-test.com"), "0xced", "hunter2");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Cli.Wrap("security")
            .WithArguments(new[]
            {
                "delete-keychain",
                _keychainName
            })
            .ExecuteAsync();
    }
}
