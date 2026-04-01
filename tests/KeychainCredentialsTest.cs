using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace KeychainCredentialsLib.Tests;

[Collection("Keychain collection")]
[SupportedOSPlatform("macOS")]
public class KeychainCredentialsTest(KeychainFixture keychain)
{
    [SkippableFact]
    public void GetCredential_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        Action action = () => keychainCredentials.GetCredential(null!, "");

        // Assert
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("uri");
    }

    [SkippableFact]
    public void GetCredential_DoesNotExistInKeychain_ReturnsNull()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.not-in-keychain.com"), "");

        // Assert
        credential.Should().BeNull();
    }

    [SkippableTheory]
    [CombinatorialData]
    public void GetCredential_SelectsNonExistingUserName_ReturnsNull(
        [CombinatorialValues(-1, 0, 1, 2)] int limit,
        [CombinatorialValues("unknown", null)] string userName,
        [CombinatorialValues("https://www.keychain-credentials-test.com", "https://www.not-in-keychain.com")] string uri)
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials(new UserNameSelection(userName, limit));

        // Act
        var credential = keychainCredentials.GetCredential(new Uri(uri), "");

        // Assert
        credential.Should().BeNull();
    }

    [SkippableFact]
    public void TryGetCredential_DoesNotExistInKeychain_ReturnsFalseAndNotFoundReason()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var success = keychainCredentials.TryGetCredential(new Uri("https://www.not-in-keychain.com"), "", out var credential, out var reason);

        // Assert
        success.Should().BeFalse();
        credential.Should().BeNull();
        reason.Should().Be(UnavailabilityReason.NotFound);
    }

    [InteractiveFact]
    public void GetCredential_ExistsInKeychain_ReturnsCredential()
    {
        // Arrange
        var keychainCredentials = new KeychainCredentials();

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.keychain-credentials-test.com"), "");

        // Assert
        credential.Should().BeEquivalentTo(new NetworkCredential("0xced", "hunter2"));
    }

    [InteractiveFact]
    public async Task GetCredential_HasMultipleUsersInKeychain_ReturnsMatchingCredential()
    {
        // Arrange
        await keychain.AddInternetPasswordAsync(new Uri("https://www.multiple-users.com"), "userA", "p@$$w0rdA");
        await keychain.AddInternetPasswordAsync(new Uri("https://www.multiple-users.com"), "userB", "p@$$w0rdB");
        var keychainCredentials = new KeychainCredentials(new FirstUserNameSelection("userB"));

        // Act
        var credential = keychainCredentials.GetCredential(new Uri("https://www.multiple-users.com"), "");

        // Assert
        credential.Should().BeEquivalentTo(new NetworkCredential("userB", "p@$$w0rdB"));
    }

    private class UserNameSelection(string? userName, int userNamesLimit) : IUserNameSelection
    {
        public int UserNamesLimit { get; } = userNamesLimit;

        public string? SelectUserName(Uri uri, string? authType, IReadOnlyCollection<string> userNames)
        {
            _ = uri;
            _ = authType;
            return userName;
        }
    }
}