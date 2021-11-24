using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

[SupportedOSPlatform("macOS")]
public class KeychainCredentialsTest : IClassFixture<KeychainFixture>
{
    private readonly KeychainFixture _keychainFixture;

    public KeychainCredentialsTest(KeychainFixture keychainFixture)
    {
        _keychainFixture = keychainFixture ?? throw new ArgumentNullException(nameof(keychainFixture));
    }

    [Fact]
    public async Task CredentialExistsInKeychain()
    {
        // Arrange
        await _keychainFixture.AddInternetPasswordAsync(new Uri("https://www.keychain-credentials-test.com"), "0xced", "hunter2");
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.keychain-credentials-test.com"), "");

        // Assert
        credential.Should().BeEquivalentTo(new NetworkCredential("0xced", "hunter2"));
    }

    [Fact]
    public void CredentialDoesNotExistInKeychain()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.not-in-keychain.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    [Fact]
    public async Task CredentialExistsInKeychainSelectsNull()
    {
        // Arrange
        await _keychainFixture.AddInternetPasswordAsync(new Uri("https://www.null-username-selection.com"), "0xced", "hunter2");
        var keychainCredentials = new KeychainCredentials(new NullUserNameSelection());

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.null-username-selection.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    [Fact]
    public void CredentialDoesNotExistInKeychainSelectsNull()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials(new NullUserNameSelection());

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.not-in-keychain.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    [Fact]
    public void CredentialDoesNotExistInKeychainSelectsMe()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials(new MeUserNameSelection());

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.not-in-keychain.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    private class NullUserNameSelection : IUserNameSelection
    {
        public string? SelectUserName(Uri uri, string authType, IReadOnlyCollection<string> userNames) => null;
    }

    private class MeUserNameSelection : IUserNameSelection
    {
        public string SelectUserName(Uri uri, string authType, IReadOnlyCollection<string> userNames) => "me";
    }
}